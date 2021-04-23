using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Respawning;
using GameCore;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CallCmdRegisterEscape))]
    public static class CharacterClassManager_CallCmdRegisterEscape
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
				if (!__instance._interactRateLimit.CanExecute(true))
					return false;

				if (Vector3.Distance(__instance.transform.position, __instance.GetComponent<Escape>().worldPosition) >= (Escape.radius * 2))
					return false;

				Player player = PlayersList.GetPlayer(__instance._hub);

				if (player == null)
					return true;

				if (player.PlayerLock)
					return false;

				bool flag = false;
				Handcuffs handcuffs = player.ReferenceHub.handcuffs;

				if (handcuffs.ForceCuff && CharacterClassManager.ForceCuffedChangeTeam)
					flag = true;

				if (handcuffs.CufferId >= 0 && CharacterClassManager.CuffedChangeTeam)
				{
					CharacterClassManager characterClassManager = ReferenceHub.GetHub(handcuffs.GetCuffer(handcuffs.CufferId)).characterClassManager;

					if (player.Role == RoleType.Scientist && (characterClassManager.CurClass == RoleType.ChaosInsurgency || characterClassManager.CurClass == RoleType.ClassD))
						flag = true;

					if (player.Role == RoleType.ClassD && (characterClassManager.CurRole.team == Team.MTF || characterClassManager.CurClass == RoleType.Scientist))
						flag = true;

				}

				RespawnTickets singleton = RespawnTickets.Singleton;
				Team team = __instance.CurRole.team;

				RoleType newRole = RoleType.ChaosInsurgency;

				if (team != Team.RSC)
				{
					if (team == Team.CDP)
					{
						if (flag)
						{
							newRole = RoleType.NtfCadet;
						}
						else
                        {
							newRole = RoleType.ChaosInsurgency;
                        }
					}
				}
				else
				{
					if (flag)
					{
						newRole = RoleType.ChaosInsurgency;
					}
					else
                    {
						newRole = RoleType.NtfScientist;
                    }
				}

				CheckEscapeEvent ev = new CheckEscapeEvent(player, newRole, true);
				EventManager.Trigger<IHandlerCheckEscape>(ev);
				Custom.Items.API.CustomItem.EventHandling.OnCheckEscape(ev);

				if (!ev.Allow)
					return false;

				__instance.SetPlayersClass(ev.NewRole, player.GameObject, false, true);

				if (team != Team.RSC)
				{
					if (team == Team.CDP)
					{
						if (flag)
						{
							RoundSummary.escaped_scientists++;
							singleton.GrantTickets(SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_classd_cuffed_count", 1), false);
							__instance.GetComponent<Escape>().TargetShowEscapeMessage(__instance.connectionToClient, true, true);

							return false;
						}

						RoundSummary.escaped_ds++;
						singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_classd_count", 1), false);
						__instance.GetComponent<Escape>().TargetShowEscapeMessage(__instance.connectionToClient, true, false);

						return false;
					}
				}
				else
				{
					if (flag)
					{
						RoundSummary.escaped_ds++;
						singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_scientist_cuffed_count", 2), false);
						__instance.GetComponent<Escape>().TargetShowEscapeMessage(__instance.connectionToClient, false, true);

						return false;
					}

					RoundSummary.escaped_scientists++;
					singleton.GrantTickets(SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_scientist_count", 1), false);
					__instance.GetComponent<Escape>().TargetShowEscapeMessage(__instance.connectionToClient, false, false);

					return false;
				}

				return false;
            }
            catch (Exception e)
            {
				Patcher.Log(typeof(CharacterClassManager_CallCmdRegisterEscape), e);
                return true;
            }
        }
    }
}
