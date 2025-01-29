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

namespace BS_Janitor.HarmonyPatches
{
    [HarmonyPatch(typeof(TimeExtensions), nameof(TimeExtensions.MinSecDurationText))]
    internal class TimeExtensionsPatch
    {
        static bool Prefix(float duration, ref string __result)
        {
            if (!float.IsFinite(duration) || float.IsNaN(duration))
            {
                return true;
            }

            var hours = duration.Hours();
            if (hours > 0)
            {
                __result = $"{hours}:{duration.Minutes():00}:{$"{duration.Seconds():00}"}";
                return false;
            }

            return true;
        }
    }
}
