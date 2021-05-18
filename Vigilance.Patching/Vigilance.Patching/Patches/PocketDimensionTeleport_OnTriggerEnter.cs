using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using UnityEngine;
using Harmony;
using LightContainmentZoneDecontamination;
using MapGeneration;
using GameCore;
using CustomPlayerEffects;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static class PocketDimensionTeleport_OnTriggerEnter
    {
        public static bool Prefix(PocketDimensionTeleport __instance, Collider other)
        {
            try
            {
				ReferenceHub hub = other.GetComponent<ReferenceHub>();

				if (hub != null)
				{
					Player player = PlayersList.GetPlayer(hub);

					if (player == null)
						return true;

					if (player.PlayerLock)
						return false;

					if (__instance.type == PocketDimensionTeleport.PDTeleportType.Killer || BlastDoor.OneDoor.isClosed)
					{
						PocketDieEvent dieEv = new PocketDieEvent(player, true);
						EventManager.Trigger<IHandlerPocketDie>(dieEv);

						if (!dieEv.Allow)
							return false;

						hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", DamageTypes.Pocket, 0), other.gameObject, true, true);
					}

					if (__instance.type == PocketDimensionTeleport.PDTeleportType.Exit)
					{
						__instance.tpPositions.Clear();
						bool decont = false;

						if (DecontaminationController.GetServerTime > DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton.DecontaminationPhases.Length - 2].TimeTrigger)
							decont = true;

						List<string> stringList = ConfigFile.ServerConfig.GetStringList(decont ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");

						if (stringList.Count > 0)
						{
							foreach (GameObject obj in GameObject.FindGameObjectsWithTag("RoomID"))
							{
								Rid roomId = obj.GetComponent<Rid>();

								if (roomId == null)
									continue;

								if (stringList.Contains(roomId.id, StringComparison.Ordinal))
									__instance.tpPositions.Add(obj.transform.position);
							}

							if (stringList.Contains("PORTAL"))
							{
								foreach (Scp106PlayerScript scp106PlayerScript in UnityEngine.Object.FindObjectsOfType<Scp106PlayerScript>())
								{
									if (scp106PlayerScript.portalPosition != Vector3.zero)
									{
										__instance.tpPositions.Add(scp106PlayerScript.portalPosition);
									}
								}
							}
						}

						if (__instance.tpPositions == null || __instance.tpPositions.Count == 0)
						{
							foreach (GameObject pdExit in MapCache.PdExits)
							{
								__instance.tpPositions.Add(pdExit.transform.position);
							}
						}

						Vector3 pos = __instance.tpPositions[UnityEngine.Random.Range(0, __instance.tpPositions.Count)];
						pos.y += 2f;

						PocketEscapeEvent ev = new PocketEscapeEvent(player, pos, true);
						EventManager.Trigger<IHandlerPocketEscape>(ev);

						if (!ev.Allow)
							return false;

						hub.playerMovementSync.AddSafeTime(3.25f, false);
						hub.playerMovementSync.OverridePosition(ev.Position, 0f, false);
						hub.playerEffectsController.DisableEffect<Corroding>();

						CompCache.PlayerStats.TargetAchieve(__instance.connectionToClient, "larryisyourfriend");
					}

					if (PocketDimensionTeleport.RefreshExit)
					{
						ImageGenerator.pocketDimensionGenerator.GenerateRandom();
						MapCache.RefreshPdExits();
					}
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PocketDimensionTeleport_OnTriggerEnter), e);
                return true;
            }
        }
    }
}
