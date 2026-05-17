using HarmonyLib;
using IPA;
using IPALogger = IPA.Logging.Logger;
using System.Reflection;
using BS_Janitor.Utils;

namespace BS_Janitor;

[Plugin(RuntimeOptions.SingleStartInit)]
public class Plugin
{
    internal static IPALogger? Logger { get; private set; }
    internal static Harmony? Harmony { get; private set; }
    internal static bool NativeLibLoaded { get; private set; } = false;

    [Init]
    public Plugin(IPALogger logger)
    {
        Logger = logger;
        Harmony = new("xlzs0.BS_Janitor");

        if (!(NativeLibLoaded = NativeLibraryManager.LoadLibrary("bs_janitor.dll")))
        {
            Logger?.Warn("Failed to load native lib, some patches won't be applied");
        }
    }

    [OnEnable]
    public void OnEnable()
    {
        Harmony?.PatchAll(Assembly.GetExecutingAssembly());
    }

    [OnDisable]
    public void OnDisable()
    {
        Harmony?.UnpatchSelf();
        NativeLibraryManager.FreeAllLibraries();
    }
}
