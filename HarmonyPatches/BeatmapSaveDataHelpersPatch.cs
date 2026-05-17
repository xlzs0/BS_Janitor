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

            if (Version.TryParse(span[startQuoteIndex..endQuoteIndex], out var version))
            {
                __result = version;
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
