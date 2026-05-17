using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BS_Janitor.Utils;

internal static class NativeLibraryManager
{
    private static readonly Dictionary<string, IntPtr> _loadedLibraries = [];
    private const string _libsDirectory = "Libs";
    private const string _embeddedPrefix = "BS_Janitor.Libs.";

    public static bool LoadLibrary(string libraryName)
    {
        if (_loadedLibraries.ContainsKey(libraryName))
        {
            return true;
        }

        var libraryPath = Path.Combine(_libsDirectory, libraryName);
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_embeddedPrefix + libraryName);
            using var fileStream = File.Create(libraryPath);
            stream.CopyTo(fileStream);
        }
        catch
        {
            return false;
        }

        var handle = LoadLibraryA(libraryPath);
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        _loadedLibraries.TryAdd(libraryName, handle);
        return true;
    }

    public static void FreeAllLibraries()
    {
        foreach (var (_, handle) in _loadedLibraries)
        {
            FreeLibrary(handle);
        }

        _loadedLibraries.Clear();
    }

    [DllImport("kernel32.dll")]
    internal static extern IntPtr LoadLibraryA(string lpLibFileName);

    [DllImport("kernel32.dll")]
    internal static extern bool FreeLibrary(IntPtr hLibModule);
}
