using BS_Janitor.Utils;
using HarmonyLib;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(MediaAsyncLoader), nameof(MediaAsyncLoader.LoadSpriteAsync)), HarmonyPriority(Int32.MaxValue)]
internal class MediaAsyncLoaderPatch
{
    private static async Task<Sprite> LoadSpriteAsync(string path)
    {
        var texture = await ImageLoader.Load(path, maxSize: path.Contains("CustomLevels") ? 512u : 0u);
        return Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f), 256, 0u, SpriteMeshType.FullRect, new(0, 0, 0, 0), generateFallbackPhysicsShape: false);
    }

    internal static bool Prefix(string path, ref Task<Sprite> __result)
    {
        if (path.Contains("://"))
        {
            return true;
        }

        __result = LoadSpriteAsync(path);
        return false;
    }
}
