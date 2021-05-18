using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using GameCore;
using Interactables.Interobjects.DoorUtils;
using NorthwoodLib.Pools;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdInteract))]
    public static class Scp079PlayerScript_CallCmdInteract
    {
        public static bool Prefix(Scp079PlayerScript __instance, string command, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || !__instance.iAm079 || !command.Contains(":"))
                    return false;

                Patcher.Log(typeof(Scp079PlayerScript), $"Received a command from client: {command} - {target == null}");

                string[] array = command.Split(':');

                __instance.RefreshCurrentRoom();

                if (!__instance.CheckInteractableLegitness(__instance.currentRoom, __instance.currentZone, target, true))
                    return false;

                Player player = PlayersList.GetPlayer(__instance.roles._hub);

                if (player == null)
                    return true;

                string cmd = array[0];

				// VS wouldn't stop complaining otherwise
				DoorVariant door = null;

                bool doorFound = target != null && target.TryGetComponent(out door);

                List<string> blacklist = ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();

                switch (cmd)
                {
                    case "TESLA":
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Tesla Gate Burst", __instance.abilities);

                            SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.Tesla, target, manaFromLabel, true);
                            EventManager.Trigger<IHandlerScp079Interact>(ev);

                            if (!ev.Allow)
                                return false;

                            manaFromLabel = ev.ExpCost;

                            if (manaFromLabel > __instance.curMana)
                            {
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                return false;
                            }

                            GameObject gate = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom + "/Gate");

                            if (gate != null)
                            {
                                gate.GetComponent<TeslaGate>().RpcInstantBurst();
                                __instance.AddInteractionToHistory(gate, array[0], true);
                                __instance.Mana -= manaFromLabel;
                                return false;
                            }

                            break;
                        }

                    case "ELEVATORTELEPORT":
                        {
							float manaFromLabel = __instance.GetManaFromLabel("Elevator Teleport", __instance.abilities);

							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.ElevatorTeleport, target, manaFromLabel, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							manaFromLabel = ev.ExpCost;

							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								return false;
							}

							Camera079 camera = null;

							foreach (Scp079Interactable scp079Interactable in __instance.nearbyInteractables)
							{
								if (scp079Interactable.type == Scp079Interactable.InteractableType.ElevatorTeleport)
								{
									camera = scp079Interactable.optionalObject.GetComponent<Camera079>();
								}
							}

							if (camera != null)
							{
								__instance.RpcSwitchCamera(camera.cameraId, false);
								__instance.Mana -= manaFromLabel;
								__instance.AddInteractionToHistory(target, array[0], true);
                                return false;
							}

							break;
                        }

                    case "LOCKDOWN":
                        {
							if (AlphaWarheadController.Host.inProgress)
								return false;

							float manaFromLabel = __instance.GetManaFromLabel("Room Lockdown", __instance.abilities);

							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.Lockdown, target, manaFromLabel, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							manaFromLabel = ev.ExpCost;

							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								return false;
							}

							GameObject room = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom);

							if (room != null)
							{
								List<Scp079Interactable> interactables = ListPool<Scp079Interactable>.Shared.Rent();

								foreach (Scp079Interactable interactable in Interface079.singleton.allInteractables)
								{
									if (interactable != null)
									{
										foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in interactable.currentZonesAndRooms)
										{
											if (zoneAndRoom.currentRoom == __instance.currentRoom && zoneAndRoom.currentZone == __instance.currentZone 
												&& interactable.transform.position.y - 100f < __instance.currentCamera.transform.position.y 
												&& !interactables.Contains(interactable))
											{
												interactables.Add(interactable);
											}
										}
									}
								}

								GameObject interObject = null;

								foreach (Scp079Interactable inter in interactables)
								{
									Scp079Interactable.InteractableType type = inter.type;

									if (type != Scp079Interactable.InteractableType.Door)
									{
										if (type == Scp079Interactable.InteractableType.Lockdown)
										{
											interObject = inter.gameObject;
										}
									}
									else if (inter.TryGetComponent(out DoorVariant variant) && (variant is IDamageableDoor damagable) && damagable != null && damagable.IsDestroyed)
									{
										return false;
									}
								}

								if (interactables.Count == 0 || interObject == null || __instance._scheduledUnlocks.Count > 0)
									return false;

								HashSet<DoorVariant> doors = new HashSet<DoorVariant>();

								for (int i = 0; i < interactables.Count; i++)
                                {
									var inter = interactables[i];

									if (inter.TryGetComponent(out DoorVariant variant))
                                    {
										if (!(variant.ActiveLocks == 0))
                                        {
											DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)variant.ActiveLocks);

											if (mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.ScpOverride))
                                            {
												if (variant.TargetState)
													variant.NetworkTargetState = false;

												variant.ServerChangeLock(DoorLockReason.Lockdown079, true);
												doors.Add(variant);
                                            }
                                        }
                                    }
                                }

								if (doors.Count != 0)
									__instance._scheduledUnlocks.Add(Time.realtimeSinceStartup + 10f, doors);


								ListPool<Scp079Interactable>.Shared.Return(interactables);

								foreach (FlickerableLightController flickerableLightController in room.GetComponentsInChildren<FlickerableLightController>())
								{
									if (flickerableLightController != null)
									{
										flickerableLightController.ServerFlickerLights(8f);
									}
								}

								__instance.AddInteractionToHistory(interObject, array[0], true);
								__instance.Mana -= manaFromLabel;
								return false;
							}

							break;
                        }

                    case "SPEAKER":
                        {
							string speakerTag = __instance.currentZone + "/" + __instance.currentRoom + "/Scp079Speaker";

							Patcher.Log(typeof(Scp079PlayerScript_CallCmdInteract), $"Trying to find speaker at path: {speakerTag}");

							GameObject speaker = GameObject.Find(speakerTag);

							float manaFromLabel = __instance.GetManaFromLabel("Speaker Start", __instance.abilities);

							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.Speaker, target, manaFromLabel, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							manaFromLabel = ev.ExpCost;

							if (manaFromLabel * 1.5f > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								return false;
							}

							if (speaker != null)
							{
								__instance.Mana -= manaFromLabel;
								__instance.Speaker = speakerTag;
								__instance.AddInteractionToHistory(speaker, array[0], true);
								return false;
							}

							break;
                        }

                    case "ELEVATORUSE":
                        {
							float manaFromLabel = __instance.GetManaFromLabel("Elevator Use", __instance.abilities);

							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.ElevatorUse, target, manaFromLabel, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							manaFromLabel = ev.ExpCost;

							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								return false;
							}

							string elevatorName = string.Empty;

							if (array.Length > 1)
								elevatorName = array[1];

							foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
							{
								if (lift.elevatorName == elevatorName && lift.UseLift())
								{
									__instance.Mana -= manaFromLabel;

									bool manaAdded = false;

									foreach (Lift.Elevator elevator in lift.elevators)
									{
										__instance.AddInteractionToHistory(elevator.door.GetComponentInParent<Scp079Interactable>().gameObject, array[0], !manaAdded);

										manaAdded = true;
									}
								}
							}

							break;
                        }

                    case "DOORLOCK":
                        {
							if (AlphaWarheadController.Host.inProgress || !doorFound)
								return false;

							if (door.TryGetComponent(out DoorNametagExtension tag) && blacklist != null && blacklist.Count > 0 && blacklist.Contains(tag.GetName))
								return false;

							if (((DoorLockReason)door.ActiveLocks).HasFlagFast(DoorLockReason.Regular079))
							{
								if (__instance.lockedDoors.Contains(door.netId))
								{
									__instance.lockedDoors.Remove(door.netId);

									door.ServerChangeLock(DoorLockReason.Regular079, false);
								}

								return false;
							}

							float manaFromLabel = __instance.GetManaFromLabel("Door Lock Minimum", __instance.abilities);

							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.Door, target, manaFromLabel, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							manaFromLabel = ev.ExpCost;

							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								return false;
							}

							if (!__instance.lockedDoors.Contains(door.netId))
								__instance.lockedDoors.Add(door.netId);

							door.ServerChangeLock(DoorLockReason.Regular079, true);

							__instance.AddInteractionToHistory(door.gameObject, array[0], true);

							__instance.Mana -= manaFromLabel;

							break;
                        }

                    case "STOPSPEAKER":
                        {
							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.Speaker, target, 0f, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							__instance.Speaker = string.Empty;

							if (ev.ExpCost != 0f)
								__instance.Mana -= ev.ExpCost;

							break;
                        }

                    case "DOOR":
                        {
							if (AlphaWarheadController.Host.inProgress || !doorFound)
								return false;

							if (door.TryGetComponent(out DoorNametagExtension tag) && blacklist != null && blacklist.Count > 0 && blacklist.Contains(tag.GetName))
								return false;

							string perms = door.RequiredPermissions.RequiredPermissions.ToString();

							float manaFromLabel = __instance.GetManaFromLabel("Door Interaction " + (perms.Contains(",") ? perms.Split(',')[0] : perms), __instance.abilities);

							SCP079InteractEvent ev = new SCP079InteractEvent(player, Scp079Interactable.InteractableType.Door, target, manaFromLabel, true);
							EventManager.Trigger<IHandlerScp079Interact>(ev);

							if (!ev.Allow)
								return false;

							manaFromLabel = ev.ExpCost;

							if (manaFromLabel > __instance.curMana)
							{
								__instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
								return false;
							}

							bool targetState = door.TargetState;

							door.ServerInteract(__instance.roles._hub, 0);

							if (targetState != door.TargetState)
							{
								__instance.Mana -= manaFromLabel;
								__instance.AddInteractionToHistory(target, array[0], true);
								return false;
							}

							break;
                        }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp079PlayerScript), e);
                return true;
            }
        }
    }
}
