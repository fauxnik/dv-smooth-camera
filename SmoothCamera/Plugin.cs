using BepInEx;
using System;
using UnityEngine;

namespace SmoothCamera
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
#region Lifecycle
		private void Awake()
		{
			SetupLogging(Logger);

			Configuration.Bind(Config);

			SmoothTracking.SetupSmoothedCamera();

			Logger.LogInfo($"Plugin [{PluginInfo.PLUGIN_GUID}|{PluginInfo.PLUGIN_NAME}|{PluginInfo.PLUGIN_VERSION}] is loaded!");
		}

		private void Destroy()
		{
			SmoothTracking.TeardownSmoothedCamera();

			Logger.LogInfo($"Plugin [{PluginInfo.PLUGIN_GUID}|{PluginInfo.PLUGIN_NAME}|{PluginInfo.PLUGIN_VERSION}] is unloaded!");
		}
#endregion

#region Logging
		private static void SetupLogging(BepInEx.Logging.ManualLogSource logger)
		{
			LogDebug = messageFactory => { Log(obj => { logger.LogDebug(obj); }, LogChannel.Debug, messageFactory); };
			LogInfo = messageFactory => { Log(obj => { logger.LogInfo(obj); }, LogChannel.Info, messageFactory); };
			LogWarning = messageFactory => { Log(obj => { logger.LogWarning(obj); }, LogChannel.Warn, messageFactory); };
			LogError = messageFactory => { Log(obj => { logger.LogError(obj); }, LogChannel.Error, messageFactory); };
		}

		private static void Log(Action<object> doLog, LogChannel requestedChannel, Func<object> messageFactory)
		{
			if (!Configuration.IsLogChannelActive(requestedChannel) && !Configuration.IsLogChannelActive(LogChannel.All)) { return; }

			var message = messageFactory();
			doLog(message);
		}

		public static Action<Func<object>> LogDebug = _ => {};
		public static Action<Func<object>> LogInfo = _ => {};
		public static Action<Func<object>> LogWarning = _ => {};
		public static Action<Func<object>> LogError = _ => {};
#endregion
	}
}
