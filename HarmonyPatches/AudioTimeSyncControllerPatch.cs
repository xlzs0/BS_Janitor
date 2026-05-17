using HarmonyLib;
using System;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Awake)), HarmonyPriority(Int32.MinValue)]
internal class AudioTimeSyncControllerPatch
{
    internal static void Prefix(AudioTimeSyncController __instance) => __instance._forcedSyncDeltaTime = 0.25f;
}
