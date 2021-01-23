using Harmony;
using Dissonance.Integrations.MirrorIgnorance;
using System;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class PlayerStats_Roundrestart
    {
        public static bool Prefix(PlayerStats __instance)
        {
			try
			{
				Log.Add("The round is restarting!");
				CustomLiteNetLib4MirrorTransport.DelayConnections = true;
				CustomLiteNetLib4MirrorTransport.UserIdFastReload.Clear();
				RagdollManager_SpawnRagdoll.Owners.Clear();
				RagdollManager_SpawnRagdoll.Ragdolls.Clear();
				Inventory_CallCmdDropItem.Pickups.Clear();
				foreach (Player player in Server.Players)
                {
					Environment.OnPlayerLeave(player, out bool d);
                }
				Server.PlayerList.Reset();
				IdleMode.PauseIdleMode = true;

				foreach (MirrorIgnorancePlayer mip in UnityEngine.Object.FindObjectsOfType<MirrorIgnorancePlayer>())
                {
					mip.OnDisable();
                }

				if (CustomNetworkManager.EnableFastRestart && ConfigManager.FastRestart)
				{
					foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
					{
						if (!referenceHub.isDedicatedServer)
						{
							try
							{
								CustomLiteNetLib4MirrorTransport.UserIdFastReload.Add(referenceHub.characterClassManager.UserId);
							}
							catch (Exception ex)
							{
								ServerConsole.AddLog("Exception occured during processing online player list for Fast Restart: " + ex.Message, ConsoleColor.Yellow);
							}
						}
					}

					__instance.RpcFastRestart();
					PlayerStats.StaticChangeLevel(false);
				}
				else
				{
					if (ServerStatic.StopNextRound == ServerStatic.NextRoundAction.DoNothing)
						__instance.RpcRoundrestart(PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000f, true);
					__instance.Invoke("ChangeLevel", 2.5f);
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add(e);
				return true;
            }
		}
    }
}
