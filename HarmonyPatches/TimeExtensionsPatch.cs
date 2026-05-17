using HarmonyLib;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(TimeExtensions), nameof(TimeExtensions.MinSecDurationText))]
internal class TimeExtensionsPatch
{
    internal static bool Prefix(float duration, ref string __result)
    {
        if (!float.IsFinite(duration))
        {
            return true;
        }

        var hours = duration.Hours();
        if (hours > 0)
        {
            __result = $"{hours}:{duration.Minutes():00}:{duration.Seconds():00}";
            return false;
        }

        return true;
    }
}
