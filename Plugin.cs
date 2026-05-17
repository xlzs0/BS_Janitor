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

    [Init]
    public Plugin(IPALogger logger)
    {
        Logger = logger;
        Harmony = new("xlzs0.BS_Janitor");
    }

    [OnEnable]
    public void OnEnable()
    {
        NativeLibraryManager.LoadLibrary("bs_janitor.dll");
        Harmony?.PatchAll(Assembly.GetExecutingAssembly());
    }

    [OnDisable]
    public void OnDisable()
    {
        Harmony?.UnpatchSelf();
        NativeLibraryManager.FreeAllLibraries();
    }
}
