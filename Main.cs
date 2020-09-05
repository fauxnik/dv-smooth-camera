using AwesomeTechnologies.VegetationSystem;
using UnityEngine;
using UnityModManagerNet;

namespace SmoothCamera
{
    [EnableReloading]
    static class Main
    {
        public static Settings settings;

        private static bool isInitialized = false;
        private static bool isActive = false;
        private static bool isActivationRequested = false;

        static bool OnLoad(UnityModManager.ModEntry modEntry)
        {
            try { settings = Settings.Load<Settings>(modEntry); } catch { }

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;

            isActivationRequested = modEntry.Active;

            return true;
        }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            if (isActive) { SmoothTracking.TeardownSmoothedCamera(); }

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool isTogglingOn)
        {
            isActivationRequested = isTogglingOn;

            return true;
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float deltaMs)
        {
            if (!isInitialized)
            {
                var mainCam = Camera.main;
                var veggieSys = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
                if (mainCam == null || veggieSys == null) { return; }

                var proSkyModEntry = UnityModManager.FindMod("ProceduralSkyMod");
                var isProSkyModActive = proSkyModEntry != null && proSkyModEntry.Loaded && proSkyModEntry.Active;
                if (isProSkyModActive)
                {
                    var skyCam = GameObject.Find("SkyCam");
                    var clearCam = GameObject.Find("ClearCam");
                    if (skyCam == null || clearCam == null) { return; }
                }

                isInitialized = true;
            }

            if (isActivationRequested && !isActive)
            {
                SmoothTracking.SetupSmoothedCamera();
                isActive = true;
            }
            else if (isActive && !isActivationRequested)
            {
                SmoothTracking.TeardownSmoothedCamera();
                isActive = false;
            }
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) { settings.Draw(modEntry); }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry) { settings.Save(modEntry); }

        public static void Log(object message) { if (settings.isLoggingEnabled) { Debug.Log(message); } }
        public static void LogWarning(object message) { if (settings.isLoggingEnabled) { Debug.LogWarning(message); } }
        public static void LogError(object message) { if (settings.isLoggingEnabled) { Debug.LogError(message); } }
    }
}
