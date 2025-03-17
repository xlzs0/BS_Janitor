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

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BS_Janitor.Utils
{
    internal static class ImageHelpers
    {
        public class Image
        {
            public ulong Width;
            public ulong Height;
            public ulong Channels;
            public byte[] Data;
        }

        internal static unsafe Image LoadImage(string path, uint maxSize = 0)
        {
            byte* ptr = null;
            try
            {
                UInt64 channels = 0;
                UInt64 width = 0;
                UInt64 height = 0;

                ptr = load_image(path, ref channels, ref width, ref height, maxSize);
                if (ptr == null)
                {
                    return null;
                }

                Image image = new()
                {
                    Width = width,
                    Height = height,
                    Channels = channels,
                    Data = new byte[width * height * channels]
                };
                Marshal.Copy(new IntPtr(ptr), image.Data, 0, image.Data.Length);

                return image;
            }
            finally
            {
                if (ptr != null)
                {
                    mem_free(ptr);
                }
            }
        }

        internal static TextureFormat GetTextureFormat(Image image)
        {
            return image.Channels switch
            {
                3 => TextureFormat.RGB24,
                4 => TextureFormat.RGBA32,
                _ => TextureFormat.RGBA32
            };
        }

        [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern byte* load_image([MarshalAs(UnmanagedType.LPWStr)] string path, ref UInt64 channels, ref UInt64 width, ref UInt64 height, UInt64 max_size);

        [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void mem_free(void* ptr);
    }
}
