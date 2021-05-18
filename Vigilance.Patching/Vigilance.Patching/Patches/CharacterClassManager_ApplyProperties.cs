using System;

using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;

using UnityEngine;
using Harmony;
using Respawning;
using Respawning.NamingRules;

using Mirror;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ApplyProperties))]
    public static class CharacterClassManager_ApplyProperties
    {
        public static bool Prefix(CharacterClassManager __instance, bool lite = false, bool escape = false)
        {
			try
			{
				Player player = PlayersList.GetPlayer(__instance._hub);

				if (player == null)
					return true;

				Role curRole = __instance.CurRole;

				if (!__instance._wasAnytimeAlive && __instance.CurClass != RoleType.Spectator && __instance.CurClass != RoleType.None)
					__instance._wasAnytimeAlive = true;

				__instance.InitSCPs();
				__instance.AliveTime = 0f;
				Team team = curRole.team;

				if (team - Team.RSC <= 1)
					__instance.EscapeStartTime = (int)Time.realtimeSinceStartup;

				Inventory inventory = __instance._hub.inventory;
				try
				{
					__instance._hub.footstepSync.SetLoudness(curRole.team, curRole.roleId.Is939());
				}
				catch
				{
				}

				if (NetworkServer.active)
				{
					Handcuffs handcuffs = __instance._hub.handcuffs;

					handcuffs.ClearTarget();

					handcuffs.NetworkCufferId = -1;
					handcuffs.NetworkForceCuff = false;

					SpawnableTeamType spawnableTeamType;

					string[] array;

					if (curRole.roleId != RoleType.Spectator && RespawnManager.CurrentSequence() != RespawnManager.RespawnSequencePhase.SpawningSelectedTeam && UnitNamingManager.RolesWithEnforcedDefaultName.TryGetValue(curRole.roleId, out spawnableTeamType) && RespawnManager.Singleton.NamingManager.TryGetAllNamesFromGroup((byte)spawnableTeamType, out array) && array.Length != 0)
					{
						__instance.NetworkCurSpawnableTeamType = (byte)spawnableTeamType;
						__instance.NetworkCurUnitName = array[0];
					}
					else if (__instance.CurSpawnableTeamType != 0)
					{
						__instance.NetworkCurSpawnableTeamType = 0;
						__instance.NetworkCurUnitName = string.Empty;
					}
				}
				if (curRole.team != Team.RIP)
				{
					if (!lite)
					{
						Vector3 constantRespawnPoint = NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
						if (constantRespawnPoint != Vector3.zero)
						{
							__instance._pms.OnPlayerClassChange(constantRespawnPoint, 0f);
							__instance._pms.IsAFK = true;
						}
						else
						{
							GameObject randomPosition = CharacterClassManager._spawnpointManager.GetRandomPosition(__instance.CurClass);

							if (randomPosition != null)
							{
								__instance._pms.OnPlayerClassChange(randomPosition.transform.position, randomPosition.transform.rotation.eulerAngles.y);
								__instance._pms.IsAFK = true;
								AmmoBox component = __instance._hub.ammoBox;

								if (escape && CharacterClassManager.KeepItemsAfterEscaping)
								{
									Inventory component2 = PlayerManager.localPlayer.GetComponent<Inventory>();

									for (ushort num = 0; num < 3; num += 1)
									{
										if (component[num] >= 15U)
										{
											component2.SetPickup(component.types[num].inventoryID, component[num], randomPosition.transform.position, randomPosition.transform.rotation, 0, 0, 0, true);
										}
									}
								}

								component.ResetAmmo();
							}
							else
							{
								__instance._pms.OnPlayerClassChange(__instance.DeathPosition, 0f);
							}
						}

						if (!__instance.SpawnProtected && CharacterClassManager.EnableSP && CharacterClassManager.SProtectedTeam.Contains((int)curRole.team))
						{
							__instance.GodMode = true;
							__instance.SpawnProtected = true;
							__instance.ProtectedTime = Time.time;
						}
					}

					if (!__instance.isLocalPlayer)
					{
						__instance._hub.playerStats.maxHP = curRole.maxHP;
					}
				}

				PlayerSpawnEvent ev = new PlayerSpawnEvent(player, curRole.roleId, true);
				EventManager.Trigger<IHandlerPlayerSpawn>(ev);

				__instance.Scp0492.iAm049_2 = (__instance.CurClass == RoleType.Scp0492);
				__instance.Scp106.iAm106 = (__instance.CurClass == RoleType.Scp106);
				__instance.Scp173.iAm173 = (__instance.CurClass == RoleType.Scp173);
				__instance.Scp939.iAm939 = __instance.CurClass.Is939();
				__instance.RefreshPlyModel(RoleType.None);

				return false;
			}
			catch (Exception e)
			{
				Patcher.Log(typeof(CharacterClassManager), e);
				return true;
			}
        }
    }
}
