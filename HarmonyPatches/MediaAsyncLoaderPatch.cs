/*
 *  Copyright (C) 2025 xlzs0
 *
 *  This file is part of BS_Janitor.
 * 
 *  BS_Janitor is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published
 *  by the Free Software Foundation, either version 3 of the License,
 *  or (at your option) any later version.
 *
 *  BS_Janitor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with BS_Janitor.  If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;
using UnityEngine.Networking;
using BGLib.UnityExtension;

namespace BS_Janitor.HarmonyPatches
{
    [HarmonyPriority(Int32.MaxValue)]
    [HarmonyPatch(typeof(MediaAsyncLoader), nameof(MediaAsyncLoader.LoadSpriteAsync))]
    internal class MediaAsyncLoaderPatch
    {
        private static async Task<Sprite> LoadSpriteAsync(string path, CancellationToken cancellationToken)
        {
            using UnityWebRequest www = UnityWebRequestTexture.GetTexture(FileHelpers.GetEscapedURLForFilePath(path));
            if (await www.SendWebRequestAsync(cancellationToken) != UnityWebRequest.Result.Success)
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
            content = texture2D;
            return Sprite.Create(content, new Rect(0f, 0f, content.width, content.height), new Vector2(0.5f, 0.5f), 256f, 0u, SpriteMeshType.FullRect, new Vector4(0f, 0f, 0f, 0f), generateFallbackPhysicsShape: false);
        }

        static bool Prefix(string path, CancellationToken cancellationToken, ref Task<Sprite> __result)
        {
            if (!Config.Instance.Enabled || !Config.Instance.FasterSpriteLoading || FileHelpers.PathIsUrl(path))
            {
                return true;
            }

            __result = LoadSpriteAsync(path, cancellationToken);
            return false;
        }
    }
}
