/*
 *  Copyright (C) 2025 xlzs0
 *
 *  This file is part of BS_Janitor.
 * 
 *  BS_Janitor is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published
 *  by the Free Software Foundation, either version 3 of the License,
 *  or (at your option) any later version.
 *
 *  BS_Janitor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with BS_Janitor.  If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BS_Janitor.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.LoadBasicBeatmapDataAsync), [typeof(IBeatmapLevelData), typeof(BeatmapKey)])]
    internal class BeatmapDataLoaderPatch
    {
        private const int _maxCacheSize = 120;
        internal static readonly ConcurrentDictionary<string, BeatmapDataBasicInfo> _cache = new();
        internal static readonly ConcurrentQueue<string> _lru = new();

        static async Task<BeatmapDataBasicInfo> LoadBeatmapBasicInfo(IBeatmapLevelData beatmapLevelData, BeatmapKey beatmapKey)
        {
            var cacheKey = beatmapKey.SerializedName();
            if (_cache.TryGetValue(cacheKey, out var cachedOutput))
            {
                return cachedOutput;
            }

            string beatmapJson = beatmapLevelData.GetBeatmapString(in beatmapKey);
            if (string.IsNullOrEmpty(beatmapJson))
            {
                return null;
            }

            BeatmapDataBasicInfo result = null;
            var version = BeatmapSaveDataHelpers.GetVersion(beatmapJson);
            if (version < BeatmapSaveDataHelpers.version3)
            {
                result = await BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJsonAsync(beatmapJson);
            }
            else if (version < BeatmapSaveDataHelpers.version4)
            {
                result = await BeatmapDataLoaderVersion3.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJsonAsync(beatmapJson);
            }
            else
            {
                result = await BeatmapDataLoaderVersion4.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJsonAsync(beatmapJson);
            }

            if (result != null)
            {
                lock (_lru)
                {
                    while (_cache.Count >= _maxCacheSize)
                    {
                        if (_lru.TryDequeue(out var oldestKey))
                        { 
                            _cache.TryRemove(oldestKey, out _);
                        }
                    }

                    _cache.TryAdd(cacheKey, result);
                    _lru.Enqueue(cacheKey);
                }
            }

            return result;
        }

        static bool Prefix(IBeatmapLevelData beatmapLevelData, BeatmapKey beatmapKey, ref Task<BeatmapDataBasicInfo> __result)
        {
            if (!Config.Instance.Enabled || !Config.Instance.SpeedUpBasicDataLoading)
            {
                return true;
            }

            __result = Task.Run(() => LoadBeatmapBasicInfo(beatmapLevelData, beatmapKey));
            return false;
        }
    }
}
