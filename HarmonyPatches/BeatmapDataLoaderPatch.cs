using BS_Janitor.Utils;
using HarmonyLib;
using System.Threading.Tasks;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.LoadBasicBeatmapDataAsync), [typeof(IBeatmapLevelData), typeof(BeatmapKey)])]
internal class BeatmapDataLoaderPatch
{
    internal static bool Prefix(IBeatmapLevelData beatmapLevelData, BeatmapKey beatmapKey, ref Task<BeatmapDataBasicInfo?> __result)
    {
        __result = Task.Run(async () => await BasicBeatmapDataParser.Parse(beatmapKey, beatmapLevelData.GetBeatmapString(beatmapKey)));
        return false;
    }
}
