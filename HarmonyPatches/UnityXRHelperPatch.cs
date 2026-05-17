using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(UnityXRHelper), nameof(UnityXRHelper.GetNodePose))]
internal class UnityXRHelperPatch
{
    private readonly static Vector3[] _pos = new Vector3[2];

    internal static void Postfix(XRNode nodeType, ref Vector3 pos, bool __result)
    {
        if (!__result || (nodeType != XRNode.LeftHand && nodeType != XRNode.RightHand))
        {
            return;
        }

        var i = nodeType == XRNode.RightHand ? 1 : 0;
        pos = InputDevices.GetDeviceAtXRNode(nodeType).TryGetFeatureValue(CommonUsages.isTracked, out var tracked) && tracked
            ? _pos[i] = pos
            : _pos[i];
    }
}
