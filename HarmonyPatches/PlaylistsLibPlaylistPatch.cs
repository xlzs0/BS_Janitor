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
