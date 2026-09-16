using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Unity.Netcode;

namespace YourName.ServerMod
{
	// A server-side mod: it runs inside the Dimraeth server process. Players install nothing.
	[BepInPlugin(PluginId, "Server Mod", Version)]
	public class ServerModPlugin : BasePlugin
	{
		public const string PluginId = "com.yourname.servermod";
		public const string Version = "1.0.0";

		internal static ManualLogSource Logger;
		internal static ConfigEntry<bool> Enabled;

		public override void Load()
		{
			Logger = Log;
			// BepInEx writes BepInEx\config\com.yourname.servermod.cfg on first run.
			Enabled = Config.Bind("General", "Enabled", true, "Turn the mod on or off.");
			try
			{
				new Harmony(PluginId).PatchAll(typeof(ServerModPlugin).Assembly);
				Logger.LogInfo("[servermod] " + Version + " loaded");
			}
			catch (Exception e) { Logger.LogError("[servermod] patching failed: " + e); }
		}

		// True in the server process once it is listening. Every patch checks this so the
		// same dll is inert on a player's PC.
		internal static bool OnServer()
		{
			try { var nm = NetworkManager.Singleton; return nm != null && nm.IsServer; } catch { return false; }
		}
	}

	// Example: a hook on the server's connection callback.
	[HarmonyPatch(typeof(CustomNetworkManager), "OnClientConnected")]
	internal static class JoinPatch
	{
		private static void Postfix(ulong clientId)
		{
			if (!ServerModPlugin.Enabled.Value || !ServerModPlugin.OnServer() || clientId == NetworkManager.ServerClientId) return;
			ServerModPlugin.Logger.LogInfo("[servermod] client " + clientId + " connected");
		}
	}
}
