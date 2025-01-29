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
using System.Threading;
using System.Threading.Tasks;

namespace BS_Janitor.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.SongPlayerCrossfadeToLevel))]
    internal class LevelCollectionViewControllerPatch
    {
        static bool Prefix(LevelCollectionViewController __instance, BeatmapLevel level)
        {
            if (!Config.Instance.Enabled || !Config.Instance.OffloadAudioPreview)
            {
                return true;
            }

            __instance._crossfadeCancellationTokenSource?.Cancel();
            __instance._crossfadeCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => __instance.SongPlayerCrossfadeToLevelAsync(level, __instance._crossfadeCancellationTokenSource.Token));
            return false;
        }
    }
}
