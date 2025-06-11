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
