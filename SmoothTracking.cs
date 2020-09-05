using AwesomeTechnologies.VegetationSystem;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityModManagerNet;

namespace SmoothCamera
{
    class SmoothTracking : MonoBehaviour
    {
        public static void SetupSmoothedCamera()
        {
            if (!VRManager.IsVREnabled())
            {
                Main.LogWarning("[SmoothCamera] >>> Game running in non-VR mode. Skipping camera setup.");
                return;
            }

            if (GameObject.Find("SmoothCamera") != null)
            {
                Main.LogWarning("[SmoothCamera] >>> SmoothCamera already exists. Skipping camera setup.");
                return;
            }

            var proSkyModEntry = UnityModManager.FindMod("ProceduralSkyMod");
            var isProSkyModLoaded = proSkyModEntry != null && proSkyModEntry.Loaded && proSkyModEntry.Active;

            MainCamSetup();
            if (isProSkyModLoaded) { ProSkyCamSetup(); }
        }

        public static void TeardownSmoothedCamera()
        {
            Main.Log("[SmoothCamera] >>> Tearing down camera...");

            var smoothCam = GameObject.Find("SmoothCamera");
            if (smoothCam == null)
            {
                Main.LogWarning("[SmoothCamera] >>> SmoothCamera not found. Skipping camera teardown.");
                return;
            }

            VegetationSystemPro vegetationSystemPro = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
            vegetationSystemPro.RemoveCamera(smoothCam.GetComponent<Camera>());

            Destroy(smoothCam);

            var smoothSkyCam = GameObject.Find("SmoothSkyCamera");
            if (smoothSkyCam != null) { Destroy(smoothSkyCam); }
            var smoothClearCam = GameObject.Find("SmoothClearCamera");
            if (smoothClearCam != null) { Destroy(smoothClearCam); }
        }

        static void MainCamSetup()
        {
            Main.Log("[SmoothCamera] >>> Setting up main camera...");

            var veggieSys = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
            var smoothCam = SetupCamera("SmoothCamera", Camera.main);
            veggieSys.AddCamera(smoothCam);
        }

        static void ProSkyCamSetup()
        {
            Main.Log("[SmoothCamera] >>> Setting up ProSky cameras...");

            var skyCam = GameObject.Find("SkyCam");
            var clearCam = GameObject.Find("ClearCam");

            if (skyCam == null || clearCam == null)
            {
                Main.LogError("[SmoothCamera] >>> Could not find cameras from Procedural Sky mod!");
                return;
            }

            SetupCamera("SmoothSkyCamera", skyCam.GetComponent<Camera>());
            SetupCamera("SmoothClearCamera", clearCam.GetComponent<Camera>());
        }

        static Camera SetupCamera(string name, Camera target)
        {
            if (target == null) { return null; }
            var smoothCam = new GameObject { name = name }.AddComponent<Camera>();
            smoothCam.CopyFrom(target);
            smoothCam.stereoTargetEye = StereoTargetEyeMask.None;
            smoothCam.depth = target.depth + 20;
            smoothCam.gameObject.AddComponent<SmoothTracking>().trackedCam = target;
            return smoothCam;
        }

        void Initialize()
        {
            initialized = true;
            transform.position = trackedCam.transform.position;
            transform.rotation = trackedCam.transform.rotation;
            positionVelo = Vector3.zero;
            rotationVelo = Vector3.zero;
            lastUpdate = DateTime.Now;
        }

        void Update()
        {
            if (trackedCam == null) { return; }

            if (!initialized) { Initialize(); }

            var smoothTimePosition = Main.settings.smoothTimePosition;
            var smoothTimeRotation = Main.settings.smoothTimeRotation;
            var now = DateTime.Now;
            var deltaTime = (float)(now - lastUpdate).TotalSeconds;
            lastUpdate = now;

            transform.position
                = Vector3.SmoothDamp(transform.position, trackedCam.transform.position, ref positionVelo, smoothTimePosition, float.PositiveInfinity, deltaTime);

            var prevEuler = transform.rotation.eulerAngles;
            var trackedCamEuler = trackedCam.transform.rotation.eulerAngles;
            var nextEuler = new Vector3(
                Mathf.SmoothDampAngle(prevEuler.x, trackedCamEuler.x, ref rotationVelo.x, smoothTimeRotation, float.PositiveInfinity, deltaTime),
                Mathf.SmoothDampAngle(prevEuler.y, trackedCamEuler.y, ref rotationVelo.y, smoothTimeRotation, float.PositiveInfinity, deltaTime),
                Mathf.SmoothDampAngle(prevEuler.z, trackedCamEuler.z, ref rotationVelo.z, smoothTimeRotation, float.PositiveInfinity, deltaTime));
            transform.rotation = Quaternion.Euler(nextEuler);

            GetComponent<Camera>().fieldOfView = Main.settings.fieldOfView / XRDevice.fovZoomFactor;
        }

        public Camera trackedCam;

        Vector3 positionVelo;
        Vector3 rotationVelo;
        DateTime lastUpdate;
        bool initialized = false;
    }
}
