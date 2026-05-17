using BGLib.UnityExtension;
using BS_Janitor.Utils;
using HarmonyLib;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(MediaAsyncLoader), nameof(MediaAsyncLoader.LoadSpriteAsync)), HarmonyPriority(Int32.MaxValue)]
internal class MediaAsyncLoaderPatch
{
    internal static bool Prepare() => Plugin.NativeLibLoaded;

    private static async Task<Texture2D?> GetTexture_Slow(string path)
    {
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(FileHelpers.GetEscapedURLForFilePath(path));
        if (await www.SendWebRequest() != UnityWebRequest.Result.Success)
        {
            return null;
        }
        Texture2D content = DownloadHandlerTexture.GetContent(www);
        content.hideFlags = HideFlags.DontSave;
        Texture2D texture2D = new(content.width, content.height, content.format, mipChain: true, linear: false)
        {
            hideFlags = HideFlags.DontSave
        };
        texture2D.LoadRawTextureData(content.GetRawTextureData<byte>());
        texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
        UnityEngine.Object.Destroy(content);
        return texture2D;
    }

    private static async Task<Sprite> LoadSpriteAsync(string path)
    {
        var texture = await ImageLoader.Load(path, maxSize: path.Contains("CustomLevels") ? 512 : -1);
        texture ??= await GetTexture_Slow(path) ?? new(2, 2);
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
