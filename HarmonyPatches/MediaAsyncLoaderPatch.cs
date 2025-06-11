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
