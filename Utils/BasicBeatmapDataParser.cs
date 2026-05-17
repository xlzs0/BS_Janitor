using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BS_Janitor.Utils;

internal static class BasicBeatmapDataParser
{
    private static readonly LRUCache<string, BeatmapDataBasicInfo> _cache = new(120);

    public static async Task<BeatmapDataBasicInfo?> Parse(BeatmapKey beatmapKey, string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var cacheKey = beatmapKey.SerializedName() + json.GetHashCode();
        if (_cache.TryGet(cacheKey, out var cached))
        {
            return cached;
        }

        BeatmapDataBasicInfo? result = ParseImpl(json, fromFile: false);

        if (result == null)
        {
            var version = BeatmapSaveDataHelpers.GetVersion(json);
            if (version < BeatmapSaveDataHelpers.version3)
            {
                result = await BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJsonAsync(json);
            }
            else if (version < BeatmapSaveDataHelpers.version4)
            {
                result = await BeatmapDataLoaderVersion3.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJsonAsync(json);
            }
            else
            {
                result = await BeatmapDataLoaderVersion4.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJsonAsync(json);
            }
        }

        _cache.Add(cacheKey, result);
        return result;
    }

    public static BeatmapDataBasicInfo? ParseFromFile(string? path)
    {
        if (string.IsNullOrEmpty(path) || path.Contains("://") || !path.Contains("CustomLevels"))
        {
            return null;
        }

        return ParseImpl(path, fromFile: true);
    }

    private static BeatmapDataBasicInfo? ParseImpl(string str, bool fromFile = false)
    {
        Output output = new();

        if (fromFile && !parse_basic_data_from_file(str, ref output) || !fromFile && !parse_basic_data(str, ref output))
        {
            return null;
        }

        return new(4, (int)output.CuttableNotes, (int)output.CuttableObjects, (int)output.Obstacles, (int)output.Bombs);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Output
    {
        public UInt64 CuttableNotes;
        public UInt64 CuttableObjects;
        public UInt64 Obstacles;
        public UInt64 Bombs;
    }

    [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool parse_basic_data([MarshalAs(UnmanagedType.LPStr)] string json, ref Output output);

    [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool parse_basic_data_from_file([MarshalAs(UnmanagedType.LPWStr)] string path, ref Output output);
}
