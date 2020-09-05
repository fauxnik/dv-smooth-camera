using System;
using UnityEngine;
using UnityEngine.XR;
using AwesomeTechnologies.VegetationSystem;

namespace SmoothCamera
{
    class SmoothTracking : MonoBehaviour
    {
        public static void SetupSmoothedCamera()
        {
            Main.Log(">>> >>> >>> Setting up camera...");

            if (GameObject.Find("SmoothCamera") != null)
            {
                Main.LogWarning("SmoothCamera already exists. Skipping camera setup.");
                return;
            }

            var mainCam = Camera.main;
            VegetationSystemPro veggieSys = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
            if (mainCam == null || veggieSys == null || !VRManager.IsVREnabled())
            {
                WorldStreamingInit.LoadingFinished += DelayedSetup;
                return;
            }

            var smoothCam = new GameObject { name = "SmoothCamera" }.AddComponent<Camera>();
            smoothCam.CopyFrom(mainCam);
            smoothCam.stereoTargetEye = StereoTargetEyeMask.None;
            smoothCam.depth = mainCam.depth + 1;
            smoothCam.gameObject.AddComponent<SmoothTracking>().trackedCam = mainCam;

            veggieSys.AddCamera(smoothCam);
        }

        static void DelayedSetup()
        {
            Main.Log(">>> >>> >>> Delayed setup...");

            WorldStreamingInit.LoadingFinished -= DelayedSetup;
            SetupSmoothedCamera();
        }

        public static void TeardownSmoothedCamera()
        {
            Main.Log(">>> >>> >>> Tearing down camera...");

            var smoothCam = GameObject.Find("SmoothCamera");
            if (smoothCam == null)
            {
                Main.LogWarning("SmoothCamera not found. Skipping camera teardown.");
                return;
            }

            VegetationSystemPro vegetationSystemPro = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
            vegetationSystemPro.RemoveCamera(smoothCam.GetComponent<Camera>());

            Destroy(smoothCam);
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
