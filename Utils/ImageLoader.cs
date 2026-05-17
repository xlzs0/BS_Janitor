using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace BS_Janitor.Utils;

internal static class ImageLoader
{
    internal static async Task<Texture2D> Load(string path, uint maxSize = 0)
    {
        IntPtr ptr = IntPtr.Zero;

        try
        {
            UInt64 channels = 0, width = 0, height = 0;

            ptr = await Task.Run(() => load_image(path, ref channels, ref width, ref height, maxSize));
            if (ptr == IntPtr.Zero)
            {
                return new(2, 2);
            }

            Texture2D texture = new((int)width, (int)height, TextureFormat.RGBA32, mipChain: true)
            {
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixelDataImpl(ptr, 0, 1, (int)(width * height * channels));
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);

            return texture;
        }
        catch
        {
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                unsafe
                {
                    mem_free((void*)ptr);
                }
            }
        }

        return new(2, 2);
    }

    [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
    private static unsafe extern IntPtr load_image([MarshalAs(UnmanagedType.LPWStr)] string path, ref UInt64 channels, ref UInt64 width, ref UInt64 height, UInt64 max_size);

    [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
    private static unsafe extern void mem_free(void* ptr);
}
