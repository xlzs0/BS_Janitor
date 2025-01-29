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

namespace BS_Janitor.Utils
{
    internal static class BasicBeatmapDataParser
    {
        static public BeatmapDataBasicInfo Parse(string json)
        {
            var output = new Output();
            if (!parse_basic_data(json, ref output))
            {
                return null;
            }

            return new(4, (int)output.CuttableNotes, (int)output.CuttableObjects, (int)output.Obstacles, (int)output.Bombs);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Output
        {
            public UInt64 CuttableNotes;
            public UInt64 CuttableObjects;
            public UInt64 Obstacles;
            public UInt64 Bombs;
        }

        [DllImport("Libs/bs_janitor.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool parse_basic_data([MarshalAs(UnmanagedType.LPStr)] string json, ref Output output);
    }
}
