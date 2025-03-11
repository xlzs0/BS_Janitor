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
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BS_Janitor.Utils
{
    internal static class NativeLibraryManager
    {
        private static readonly ConcurrentDictionary<string, IntPtr> _loadedLibraries = new();
        private const string _libsDirectory = "Libs";
        private const string _embeddedPrefix = "BS_Janitor.Libs.";

        public static bool LoadLibrary(string libraryName)
        {
            Plugin.OnDisabled -= FreeAllLibraries;
            Plugin.OnDisabled += FreeAllLibraries;

            var libraryPath = Path.Combine(_libsDirectory, libraryName);

            if (_loadedLibraries.ContainsKey(libraryPath))
            {
                return true;
            }

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(_embeddedPrefix + libraryName);
            if (stream == null)
            {
                return false;
            }

            using var fileStream = File.Create(libraryPath);
            stream.CopyTo(fileStream);

            var handle = LoadLibraryA(libraryPath);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            _loadedLibraries.TryAdd(libraryPath, handle);
            return true;
        }

        public static void FreeAllLibraries()
        {
            foreach (var pair in _loadedLibraries)
            {
                if (pair.Value != IntPtr.Zero)
                {
                    FreeLibrary(pair.Value);
                }
            }

            _loadedLibraries.Clear();
        }

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibraryA(string lpLibFileName);

        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(IntPtr hLibModule);
    }
}
