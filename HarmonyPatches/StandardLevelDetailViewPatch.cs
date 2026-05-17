using BS_Janitor.Utils;
using HarmonyLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.CalculateAndSetContent))]
internal class StandardLevelDetailViewPatch
{
    internal static bool Prepare() => Plugin.NativeLibLoaded;

    private static async void InvokeSafe<A, B>(Func<A, B, Task> task, A a, B b)
    {
        try
        {
            await task(a, b);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static async Task<BeatmapDataBasicInfo?> GetBasicInfo(IBeatmapLevelData beatmapLevelData, BeatmapKey beatmapKey)
    {
        if (beatmapLevelData is FileSystemBeatmapLevelData fsLevelData)
        {
            var result = await Task.Run(() => BasicBeatmapDataParser.ParseFromFile(fsLevelData.GetDifficultyBeatmap(beatmapKey)?._beatmapPath));
            if (result != null)
            {
                return result;
            }
        }

        var json = beatmapLevelData.GetBeatmapString(beatmapKey);
        return await Task.Run(() => BasicBeatmapDataParser.Parse(beatmapKey, json));
    }

    private static async Task CalculateAndSetContentAsync(StandardLevelDetailView __instance, CancellationToken cancellationToken)
    {
        var beatmapKey = __instance.beatmapKey;
        var beatmapLevelDataVersion = await __instance._beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(beatmapKey.levelId, cancellationToken);
        var beatmapLevelData = await __instance._beatmapLevelsModel.LoadBeatmapLevelDataAsync(beatmapKey.levelId, beatmapLevelDataVersion, cancellationToken);

        if (beatmapLevelData.isError || beatmapLevelData.beatmapLevelData == null || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var songLength = 0f;
        var audioClipAsyncLoader = __instance._audioClipAsyncLoader;
        var audioClip = await audioClipAsyncLoader.LoadSong(beatmapLevelData.beatmapLevelData);

        if (audioClip != null)
        {
            songLength = audioClip.length;
            audioClipAsyncLoader.UnloadSong(beatmapLevelData.beatmapLevelData);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var beatmapDataBasicInfo = await GetBasicInfo(beatmapLevelData.beatmapLevelData, beatmapKey);
        if (beatmapDataBasicInfo == null || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        __instance.SetData(beatmapDataBasicInfo.cuttableNotesCount, beatmapDataBasicInfo.obstaclesCount, beatmapDataBasicInfo.bombsCount, songLength);
        __instance._levelParamsPanelCanvasGroupTween ??= new FloatTween(0f, 1f, value => __instance._levelParamsPanelCanvasGroup.alpha = value, 0.1f, EaseType.InSine);
        __instance._tweeningManager.RestartTween(__instance._levelParamsPanelCanvasGroupTween, __instance);
    }

    internal static bool Prefix(StandardLevelDetailView __instance)
    {
        __instance._cancellationTokenSource?.Cancel();
        __instance._cancellationTokenSource = new();
        InvokeSafe(CalculateAndSetContentAsync, __instance, __instance._cancellationTokenSource.Token);
        return false;
    }
}
