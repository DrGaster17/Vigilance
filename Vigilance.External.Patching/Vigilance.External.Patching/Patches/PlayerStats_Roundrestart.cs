using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using Harmony;
using Dissonance.Integrations.MirrorIgnorance;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class PlayerStats_Roundrestart
    {
        public static bool Prefix(PlayerStats __instance)
        {
            try
            {
				RoundRestartEvent ev = new RoundRestartEvent(PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000f);
				EventManager.Trigger<IHandlerRoundRestart>(ev);

				Patcher.Log(typeof(PlayerStats_Roundrestart), $"The round is restarting with {PlayersList.List.Count} players! Reconnection Time: {ev.ReconnectionTime}");

				CustomLiteNetLib4MirrorTransport.DelayConnections = true;
				CustomLiteNetLib4MirrorTransport.UserIdFastReload.Clear();
				IdleMode.PauseIdleMode = true;

				MirrorIgnorancePlayer[] array = UnityEngine.Object.FindObjectsOfType<MirrorIgnorancePlayer>();

				for (int i = 0; i < array.Length; i++)
					array[i].OnDisable();

				Invoke();

				if (CustomNetworkManager.EnableFastRestart)
				{
					foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
					{
						if (referenceHub.isDedicatedServer || referenceHub.characterClassManager == null || string.IsNullOrEmpty(referenceHub.characterClassManager.UserId))
							continue;

						CustomLiteNetLib4MirrorTransport.UserIdFastReload.Add(referenceHub.characterClassManager.UserId);
					}

					__instance.RpcFastRestart();
					PlayerStats.StaticChangeLevel(false);

					return false;
				}

				if (ServerStatic.StopNextRound == ServerStatic.NextRoundAction.DoNothing)
					__instance.RpcRoundrestart(ev.ReconnectionTime, true);

				__instance.Invoke("ChangeLevel", 2.5f);

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerStats_Roundrestart), e);
                return true;
            }
        }

		public static void Invoke()
        {
			SeedSynchronizer_Update.EventCalled = false;
			PlayersList.List.ForEach(x => EventManager.Trigger<IHandlerPlayerLeave>(new PlayerLeaveEvent(x)));
			PlayersList.Clear();

			PatchData.Clear();
			MapUtilities.OnRoundRestart();

			if (PluginManager.Config.ShouldReloadConfigsOnRoundRestart)
				Server.Reload();

			if (PluginManager.Config.DisableLocksOnRestart)
            {
				Round.LobbyLock = false;
				Round.RoundLock = false;
            }
        }
    }
}
