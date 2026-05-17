using HarmonyLib;
using System.Threading.Tasks;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.SongPlayerCrossfadeToLevel))]
internal class LevelCollectionViewControllerPatch
{
    internal static bool Prefix(LevelCollectionViewController __instance, BeatmapLevel level)
    {
        __instance._crossfadeCancellationTokenSource?.Cancel();
        __instance._crossfadeCancellationTokenSource = new();
        Task.Run(() => __instance.SongPlayerCrossfadeToLevelAsync(level, __instance._crossfadeCancellationTokenSource.Token));
        return false;
    }
}
