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
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"Failed to extract library {libraryName}: {ex.Message}");
            return false;
        }

        var handle = LoadLibraryA(libraryPath);
        if (handle == IntPtr.Zero)
        {
            Plugin.Logger?.Error($"Failed to load library {libraryName}");
            return false;
        }

        var addr = GetProcAddress(handle, "init");
        if (addr == IntPtr.Zero)
        {
            Plugin.Logger?.Error($"Failed to find entry point of library {libraryName}");
            FreeLibrary(handle);
            return false;
        }

        var result = Marshal.GetDelegateForFunctionPointer<InitFn>(addr)();
        if (result != NativeError.SUCCESS)
        {
            Plugin.Logger?.Error($"Failed to initialize library {libraryName}: {result}");
            FreeLibrary(handle);
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

    private delegate NativeError InitFn();

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    internal static extern IntPtr LoadLibraryA(string lpLibFileName);

    [DllImport("kernel32.dll")]
    internal static extern bool FreeLibrary(IntPtr hLibModule);
}

internal enum NativeError : int
{
    SUCCESS = 0,
    MONO_MODULE_NOT_FOUND = 1,
    MONO_FUNC_NOT_FOUND = 2
}
