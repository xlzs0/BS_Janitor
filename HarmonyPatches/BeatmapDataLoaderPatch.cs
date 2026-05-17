using BS_Janitor.Utils;
using HarmonyLib;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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


[HarmonyPatch(typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader), nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataFromSaveData))]
internal class BeatmapDataLoaderGetBeatmapDataFromSaveData
{
    private static readonly MethodInfo _insertDefaultEventsMethod = AccessTools.Method(typeof(DefaultEnvironmentEventsFactory), "InsertDefaultEvents");
    private static readonly MethodInfo _insertDefaultEventsConditionalMethod = SymbolExtensions.GetMethodInfo(() => InsertDefaultEventsConditional(null!, false));

    protected static bool Prepare() => PluginManager.GetPluginFromId("CustomJSONData") == null;

    protected static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;

        foreach (var instruction in instructions)
        {
            if (!found && instruction.opcode == OpCodes.Call && Equals(instruction.operand, _insertDefaultEventsMethod))
            {
                found = true;
                yield return new(OpCodes.Ldloc_1);
                instruction.operand = _insertDefaultEventsConditionalMethod;
            }

            yield return instruction;
        }

        if (!found)
            throw new InvalidOperationException("Couldn't find InsertDefaultEvents (call)");
    }

    private static void InsertDefaultEventsConditional(BeatmapData beatmapData, bool flag3)
    {
        if (!flag3)
            DefaultEnvironmentEventsFactory.InsertDefaultEvents(beatmapData);
    }
}
