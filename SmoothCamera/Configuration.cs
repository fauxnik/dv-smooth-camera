using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmoothCamera
{
	class Configuration {

		private static float DEFAULT_FOV = 60f;
		private static float DEFAULT_ROTATION_TIME = 0.5f;
		private static float DEFAULT_POSITION_TIME = 0f;

		public static void Bind(ConfigFile Config)
		{
			configLogChannels = Config.Bind(
				"General",
				"LogChannels",
#if DEBUG
				"All",
#else
				"Warn, Error",
#endif
				"Specifies which SmoothCamera log channels to listen to."
			);

			configCameraFOV = Config.Bind(
				"Camera",
				"FieldOfView",
				DEFAULT_FOV,
				"Specifies the field of view for the smoothed camera."
			);

			configCameraSmoothingTimeRotation = Config.Bind(
				"Camera",
				"SmoothingTime_Rotation",
				DEFAULT_ROTATION_TIME,
				"Approximately the time it will take to reach the target. A smaller value will reach the target faster."
			);

			configCameraSmoothingTimePosition = Config.Bind(
				"Camera",
				"SmoothingTime_Position",
				DEFAULT_POSITION_TIME,
				"Approximately the time it will take to reach the target. A smaller value will reach the target faster. (Values other than 0 may produce undesirable results.)"
			);
		}

		private static ConfigEntry<string>? configLogChannels;
		private static HashSet<LogChannel>? logChannels;
		public static bool IsLogChannelActive(LogChannel logChannel)
		{
			if (logChannel == LogChannel.None || configLogChannels == null) { return false; }

			if (logChannels == null)
			{
				string config = configLogChannels.Value;
				IEnumerable<string> channels = from channel in config.Split(',') where !string.IsNullOrEmpty(channel.Trim()) select channel.Trim();
				logChannels = new HashSet<LogChannel>();
				foreach(var channel in channels)
				{
					logChannels.Add(
						channel switch
						{
							"Info" => LogChannel.Info,
							"IL" => LogChannel.IL,
							"Warn" => LogChannel.Warn,
							"Error" => LogChannel.Error,
							"Debug" => LogChannel.Debug,
							_ => LogChannel.None,
						}
					);
				}
			}

			return logChannels.Contains(logChannel);
		}

		private static ConfigEntry<float>? configCameraFOV;
		public static float CameraFOV
		{
			get
			{
				float fieldOfView = DEFAULT_FOV;
				if (configCameraFOV != null) { fieldOfView = configCameraFOV.Value; }
				return Mathf.Clamp(fieldOfView, 30f, 180f);
			}
		}

		private static ConfigEntry<float>? configCameraSmoothingTimeRotation;
		public static float CameraSmoothingTimeRotation
		{
			get
			{
				float rotationTime = DEFAULT_ROTATION_TIME;
				if (configCameraSmoothingTimeRotation != null) { rotationTime = configCameraSmoothingTimeRotation.Value; }
				return Mathf.Clamp(rotationTime, 0f, float.MaxValue);
			}
		}

		private static ConfigEntry<float>? configCameraSmoothingTimePosition;
		public static float CameraSmoothingTimePosition
		{
			get
			{
				float positionTime = DEFAULT_POSITION_TIME;
				if (configCameraSmoothingTimePosition != null) { positionTime = configCameraSmoothingTimePosition.Value; }
				return Mathf.Clamp(positionTime, 0f, float.MaxValue);
			}
		}
	}
}