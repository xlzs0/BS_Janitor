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

using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using System.IO;
using System.Threading.Tasks;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(Playlist), "QueueLoadSprite")]
internal class PlaylistsLibPlaylistPatch
{
    private static async Task QueueLoadSprite(Playlist playlist)
    {
        using var stream = playlist.HasCover ? playlist.GetCoverStream() : null;
        using var downscaleStream = stream != null && stream != Stream.Null ? await Task.Run(() => Utilities.DownscaleImage(stream, 128)) : stream;
        var sprite = Utilities.GetSpriteFromStream(downscaleStream ?? Stream.Null);
        playlist._sprite = sprite;
        playlist._smallSprite = sprite;
        Playlist.OnSpriteLoaded(playlist);
    }

    internal static bool Prefix(Playlist playlist)
    {
        _ = QueueLoadSprite(playlist);
        return false;
    }
}
