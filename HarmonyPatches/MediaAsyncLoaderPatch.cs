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
using BS_Janitor.Utils;

namespace BS_Janitor.HarmonyPatches
{
    [HarmonyPriority(Int32.MaxValue)]
    [HarmonyPatch(typeof(MediaAsyncLoader), nameof(MediaAsyncLoader.LoadSpriteAsync))]
    internal class MediaAsyncLoaderPatch
    {
        private static async Task<Sprite> LoadSpriteAsync(string path, CancellationToken cancellationToken)
        {
            var image = await Task.Run(() => ImageHelpers.LoadImage(path, maxSize: path.Contains("CustomLevels") ? (uint)Config.Instance.MaxCoverSize : 0));
            if (image == null)
            {
                return Sprite.Create(new Texture2D(1, 1), new Rect(0f, 0f, 1, 1), new Vector2(0.5f, 0.5f), 256f, 0u, SpriteMeshType.FullRect, new Vector4(0f, 0f, 0f, 0f), generateFallbackPhysicsShape: false);
            }

            var texture = new Texture2D((int)image.Width, (int)image.Height, ImageHelpers.GetTextureFormat(image), mipChain: true)
            {
                hideFlags = HideFlags.DontSave
            };

            texture.SetPixelData(image.Data, 0);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 256f, 0u, SpriteMeshType.FullRect, new Vector4(0f, 0f, 0f, 0f), generateFallbackPhysicsShape: false);
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
