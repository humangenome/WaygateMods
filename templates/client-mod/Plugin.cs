using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Unity.Netcode;

namespace YourName.ClientMod
{
	// A client-side mod: installed on a player's PC by the Waygate app when a server lists
	// it, loaded only for that server. It reads replicated state and changes nothing the
	// server owns.
	[BepInPlugin(PluginId, "Client Mod", Version)]
	public class ClientModPlugin : BasePlugin
	{
		public const string PluginId = "com.yourname.clientmod";
		public const string Version = "1.0.0";

		internal static ManualLogSource Logger;
		internal static ConfigEntry<bool> Enabled;
		private static bool sConnectedLogged;

		public override void Load()
		{
			Logger = Log;
			Enabled = Config.Bind("General", "Enabled", true, "Turn the mod on or off.");
			try
			{
				new Harmony(PluginId).PatchAll(typeof(ClientModPlugin).Assembly);
				Logger.LogInfo("[clientmod] " + Version + " loaded");
			}
			catch (Exception e) { Logger.LogError("[clientmod] patching failed: " + e); }
		}

		// A connected client that is not the server. On a server this dll does nothing.
		internal static bool OnClient()
		{
			try { var nm = NetworkManager.Singleton; return nm != null && nm.IsClient && !nm.IsServer && nm.IsConnectedClient; } catch { return false; }
		}

		internal static void Tick()
		{
			if (!Enabled.Value) return;
			if (!OnClient()) { sConnectedLogged = false; return; }
			if (!sConnectedLogged) { sConnectedLogged = true; Logger.LogInfo("[clientmod] connected"); }
		}
	}

	[HarmonyPatch(typeof(TransitionManager), "Update")]
	internal static class TickPatch
	{
		private static void Postfix()
		{
			try { ClientModPlugin.Tick(); }
			catch (Exception e) { ClientModPlugin.Logger.LogWarning("[clientmod] tick: " + e.Message); }
		}
	}
}
