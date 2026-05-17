using HarmonyLib;
using System;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(DateTime), "get_Now")]
internal class DateTimePatch
{
    internal static void Postfix(ref DateTime __result)
    {
        var day = __result.Day;
        if (__result.Month == 4 && (day == 1 || day == 22))
        {
            __result = __result.AddDays(1);
        }
    }
}
