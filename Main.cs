using UnityEngine;
using UnityModManagerNet;

namespace SmoothCamera
{
    [EnableReloading]
    static class Main
    {
        public static Settings settings;

        static bool OnLoad(UnityModManager.ModEntry modEntry)
        {
            try { settings = Settings.Load<Settings>(modEntry); } catch { }

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.OnToggle = OnToggle;

            if (modEntry.Active) { SmoothTracking.SetupSmoothedCamera(); }

            return true;
        }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            if (modEntry.Active) { SmoothTracking.TeardownSmoothedCamera(); }

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool isTogglingOn)
        {
            if (isTogglingOn) { SmoothTracking.SetupSmoothedCamera(); }
            else { SmoothTracking.TeardownSmoothedCamera(); }

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) { settings.Draw(modEntry); }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) { settings.Save(modEntry); }

        public static void Log(object message) { if (settings.isLoggingEnabled) { Debug.Log(message); } }
        public static void LogWarning(object message) { if (settings.isLoggingEnabled) { Debug.LogWarning(message); } }
        public static void LogError(object message) { if (settings.isLoggingEnabled) { Debug.LogError(message); } }
    }
}
