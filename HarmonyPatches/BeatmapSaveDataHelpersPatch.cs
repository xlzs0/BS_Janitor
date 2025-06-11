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
using System;
using System.Threading.Tasks;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(BeatmapSaveDataHelpers), nameof(BeatmapSaveDataHelpers.GetVersion))]
internal class BeatmapSaveDataHelpersPatch
{
    internal static bool Prefix(ref string data, ref Version __result)
    {
        try
        {
            var span = data.AsSpan();
            var versionIndex = span.IndexOf("version");
            var colonIndex = span[versionIndex..].IndexOf(':') + versionIndex;
            var startQuoteIndex = span[colonIndex..].IndexOf('"') + colonIndex + 1;
            var endQuoteIndex = span[startQuoteIndex..].IndexOf('"') + startQuoteIndex;

            if (Version.TryParse(span[startQuoteIndex..endQuoteIndex], out Version parsedVersion))
            {
                __result = parsedVersion;
                return false;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }

        __result = BeatmapSaveDataHelpers.noVersion;
        return false;
    }
}


[HarmonyPatch(typeof(BeatmapSaveDataHelpers), nameof(BeatmapSaveDataHelpers.GetVersionAsync))]
internal class BeatmapSaveDataHelpersNoAsyncPatch
{
    internal static bool Prefix(ref string data, ref Task<Version> __result)
    {
        __result = Task.FromResult(BeatmapSaveDataHelpers.GetVersion(data));
        return false;
    }
}