using AwesomeTechnologies.VegetationSystem;
using DV.Utils;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace SmoothCamera
{
	class SmoothTracking : MonoBehaviour
	{
		static GameViewRenderMode defaultGameViewRenderMode;
		static string SMOOTH_CAMERA_KEY = "SmoothCamera";

#region Static Setup/Teardown
		public static void SetupSmoothedCamera()
		{
			WorldStreamingInit.LoadingFinished -= SetupSmoothedCamera;

			if (!VRManager.IsVREnabled())
			{
				Plugin.LogWarning(() => "Game running in non-VR mode. Skipping camera setup.");
				return;
			}

			if (GameObject.Find(SMOOTH_CAMERA_KEY) != null)
			{
				Plugin.LogWarning(() => "SmoothCamera already exists. Skipping camera setup.");
				return;
			}

			var veggieSys = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
			if (veggieSys == null)
			{
				Plugin.LogInfo(() => "Delaying camera setup until vegetation system is ready...");
				WorldStreamingInit.LoadingFinished += SetupSmoothedCamera;
				return;
			}

			PlayerManager.CameraChanged += OnCameraChanged;
			OnCameraChanged();
			defaultGameViewRenderMode = XRSettings.gameViewRenderMode;
			XRSettings.gameViewRenderMode = GameViewRenderMode.None;
		}

		public static void TeardownSmoothedCamera()
		{
			Plugin.LogInfo(() => "Tearing down camera...");

			if (GameObject.Find(SMOOTH_CAMERA_KEY)?.GetComponent<Camera>() is Camera smoothCam)
			{
				TeardownCamera(smoothCam);
			}
			else
			{
				Plugin.LogWarning(() => "SmoothCamera not found. Skipping camera teardown.");
			}

			XRSettings.gameViewRenderMode = defaultGameViewRenderMode;
		}

		static void OnCameraChanged()
		{
			if (GameObject.Find(SMOOTH_CAMERA_KEY)?.GetComponent<Camera>() is Camera smoothCam)
			{
				Plugin.LogInfo(() => "Removing stale player camera...");
				TeardownCamera(smoothCam);
			}

			Plugin.LogInfo(() => "Setting up player camera...");
			SetupCamera(SMOOTH_CAMERA_KEY, PlayerManager.PlayerCamera);
		}

		static Camera? SetupCamera(string name, Camera? target)
		{
			if (target == null)
			{
				Plugin.LogError(() => $"Unexpected null target camera. Can't setup camera {name}.");
				return null;
			}

			VegetationSystemPro veggieSys = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
			if (veggieSys == null)
			{
				Plugin.LogError(() => $"Vegetation system not found. Can't setup camera {name}.");
				return null;
			}

			var smoothCam = new GameObject { name = name }.AddComponent<Camera>();
			smoothCam.CopyFrom(target);
			smoothCam.stereoTargetEye = StereoTargetEyeMask.None;
			smoothCam.depth = target.depth + 20;
			smoothCam.gameObject.AddComponent<SmoothTracking>().trackedCam = target;

			veggieSys.AddCamera(smoothCam);

			return smoothCam;
		}

		static void TeardownCamera(Camera? smoothCam)
		{
			if (smoothCam == null)
			{
				Plugin.LogError(() => "Unexpected null value. Can't teardown smoothed camera.");
				return;
			}

			VegetationSystemPro veggieSys = UnityEngine.Object.FindObjectOfType<VegetationSystemPro>();
			if (veggieSys == null)
			{
				Plugin.LogWarning(() => "Vegetation system not found. Unexpected behavior may result.");
			}
			else
			{
				veggieSys.RemoveCamera(smoothCam);
			}

			Destroy(smoothCam.gameObject);
		}
#endregion

#region MonoBehaviour Lifecycle
		void Initialize()
		{
			if (trackedCam == null)
			{
				Plugin.LogError(() => "Attempted to initialize smooth tracking without specifying a tracked camera.");
				return;
			}

			initialized = true;
			transform.position = trackedCam.transform.position;
			transform.rotation = trackedCam.transform.rotation;
			positionVelo = Vector3.zero;
			rotationVelo = Vector3.zero;
			lastUpdate = DateTime.Now;
			SingletonBehaviour<WorldMover>.Instance.WorldMoved += OnWorldMoved;
		}

		void OnDestroy()
		{
			SingletonBehaviour<WorldMover>.Instance.WorldMoved -= OnWorldMoved;
		}

		void OnWorldMoved(WorldMover _, Vector3 moveVector)
		{
			transform.position = transform.position - moveVector;
		}

		void Update()
		{
			if (trackedCam == null) { return; }

			if (!initialized) { Initialize(); }

			var smoothTimePosition = Configuration.CameraSmoothingTimePosition;
			var smoothTimeRotation = Configuration.CameraSmoothingTimeRotation;
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

			GetComponent<Camera>().fieldOfView = Configuration.CameraFOV / XRDevice.fovZoomFactor;
		}

		public Camera? trackedCam;

		Vector3 positionVelo;
		Vector3 rotationVelo;
		DateTime lastUpdate;
		bool initialized = false;
	}
#endregion
}
