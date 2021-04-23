using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using UnityEngine;
using Harmony;
using GameCore;
using MEC;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    public static class RoundSummary_Start
    {
        public static bool Prefix(RoundSummary __instance)
        {
            try
            {
				RoundSummary.roundTime = 0;
				RoundSummary.kills_by_scp = 0;
				RoundSummary.escaped_ds = 0;
				RoundSummary.escaped_scientists = 0;
				RoundSummary.changed_into_zombies = 0;
				RoundSummary.Kills = 0;
				RoundSummary.singleton = __instance;
				__instance._keepRoundOnOne = !ConfigFile.ServerConfig.GetBool("end_round_on_one_player", false);
				Timing.RunCoroutine(_CustomProcess(__instance), Segment.FixedUpdate);
				return false;
			}
            catch (Exception e)
            {
				Patcher.Log(typeof(RoundSummary_Start), e);
				return true;
            }
        }

        private static IEnumerator<float> _CustomProcess(RoundSummary instance)
        {
			while (instance != null)
			{
				while (RoundSummary.RoundLock || !RoundSummary.RoundInProgress() || (instance._keepRoundOnOne && PlayerManager.players.Count < 2))
					yield return 0f;

				RoundSummary.SumInfo_ClassList newList = new RoundSummary.SumInfo_ClassList();

				foreach (ReferenceHub hub in ReferenceHub.GetAllHubs().Values)
				{
					if (hub.characterClassManager.Classes.CheckBounds(hub.characterClassManager.CurClass))
					{
						switch (hub.characterClassManager.CurRole.team)
						{
							case Team.SCP:
								if (hub.characterClassManager.CurClass == RoleType.Scp0492)
									newList.zombies++;
								else
									newList.scps_except_zombies++;
								break;

							case Team.MTF:
								newList.mtf_and_guards++;
								break;

							case Team.CHI:
								newList.chaos_insurgents++;
								break;

							case Team.RSC:
								newList.scientists++;
								break;

							case Team.CDP:
								newList.class_ds++;
								break;
						}
					}
				}

				newList.warhead_kills = (AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1);
				yield return float.NegativeInfinity;

				newList.time = (int)Time.realtimeSinceStartup;
				yield return float.NegativeInfinity;

				RoundSummary.roundTime = newList.time - instance.classlistStart.time;
				int num = newList.mtf_and_guards + newList.scientists;
				int num2 = newList.chaos_insurgents + newList.class_ds;
				int num3 = newList.scps_except_zombies + newList.zombies;
				float num4 = (instance.classlistStart.class_ds == 0) ? 0 : ((RoundSummary.escaped_ds + newList.class_ds) / instance.classlistStart.class_ds);
				float num5 = (instance.classlistStart.scientists == 0) ? 1 : ((RoundSummary.escaped_scientists + newList.scientists) / instance.classlistStart.scientists);

				if (newList.class_ds == 0 && num == 0)
					instance._roundEnded = true;
				else
				{
					int num6 = 0;

					if (num > 0)
						num6++;

					if (num2 > 0)
						num6++;

					if (num3 > 0)
						num6++;

					if (num6 <= 1)
						instance._roundEnded = true;
				}

				RoundSummary.LeadingTeam leadingTeam = RoundSummary.LeadingTeam.Draw;

				if (num > 0)
				{
					if (RoundSummary.escaped_ds == 0 && RoundSummary.escaped_scientists != 0)
						leadingTeam = RoundSummary.LeadingTeam.FacilityForces;
				}
				else
				{
					leadingTeam = RoundSummary.escaped_ds != 0 ? RoundSummary.LeadingTeam.ChaosInsurgency : RoundSummary.LeadingTeam.Anomalies;
				}

				CheckRoundEndEvent ev = new CheckRoundEndEvent(instance._roundEnded, (LeadingTeam)(int)leadingTeam);
				EventManager.Trigger<IHandlerCheckRoundEnd>(ev);
				instance._roundEnded = ev.Allow;
				leadingTeam = (RoundSummary.LeadingTeam)(int)ev.LeadingTeam;

				if (instance._roundEnded)
				{
					int timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
					RoundEndEvent endEv = new RoundEndEvent((LeadingTeam)(int)leadingTeam, instance.classlistStart, newList, timeToRoundRestart, true);
					Patcher.Log(typeof(RoundSummary_Start), "Triggering RoundEndEvent");
					EventManager.Trigger<IHandlerRoundEnd>(endEv);

					if (!endEv.Allow)
						yield break;

					leadingTeam = (RoundSummary.LeadingTeam)(int)endEv.LeadingTeam;
					FriendlyFireConfig.PauseDetector = true;
					string text = "Round finished! Anomalies: " + num3 + " | Chaos: " + num2 + " | Facility Forces: " + num + " | D escaped percentage: " + num4 + " | S escaped percentage: : " + num5;
					Log.Add(text);
					ServerLogs.AddLog(ServerLogs.Modules.Logger, text, ServerLogs.ServerLogType.GameEvent, false);

					for (byte i = 0; i < 75; i++)
						yield return 0f;

					Round.State = RoundState.ShowingSummary;
					instance.RpcShowRoundSummary(instance.classlistStart, newList, leadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, timeToRoundRestart);

					for (int j = 0; j < 50 * (timeToRoundRestart - 1); j++)
						yield return 0f;

					instance.RpcDimScreen();

					for (byte i = 0; i < 50; i++)
						yield return 0f;

					Round.State = RoundState.Restarting;

					// This should never be null but just in case
					if (LocalComponents.PlayerStats != null)
						LocalComponents.PlayerStats.Roundrestart();
					else
						PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
				}
			}

			yield break;
		}
    }
}
