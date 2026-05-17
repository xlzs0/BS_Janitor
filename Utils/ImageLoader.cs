using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace BS_Janitor.Utils;

internal static class ImageLoader
{
    internal static async Task<Texture2D?> Load(string path, int maxSize = -1)
    {
        IntPtr ptr = IntPtr.Zero;

        try
        {
            int width = 0, height = 0, channels = 0;

            ptr = await Task.Run(() => LoadImage_Injected(path, ref width, ref height, ref channels, maxSize));
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            Texture2D texture = new(width, height, channels == 3 ? TextureFormat.RGB24 : TextureFormat.RGBA32, mipChain: true, linear: false, createUninitialized: true)
            {
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixelDataImpl(ptr, 0, 1, width * height * channels);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);

            return texture;
        }
        catch
        {
        }
        finally
        {
            Free_Injected(ptr);
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern IntPtr LoadImage_Injected(string path, ref int width, ref int height, ref int channels, int max_size);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void Free_Injected(IntPtr ptr);
}
