using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Extensions;
using Vigilance.Custom.Items.API;
using UnityEngine;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdShoot))]
    public static class WeaponManager_CallCmdShoot
    {
		public static bool Prefix(WeaponManager __instance, GameObject target, HitBoxType hitboxType, Vector3 dir, Vector3 sourcePos, Vector3 targetPos)
        {
            try
            {
				if (!__instance._iawRateLimit.CanExecute(true))
					return false;

				int itemIndex = __instance._hub.inventory.GetItemIndex();

				if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count)
					return false;

				if (__instance.curWeapon < 0 
					|| ((__instance._reloadCooldown > 0f 
					|| __instance._fireCooldown > 0f) && !__instance.isLocalPlayer)
					|| __instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID
					|| __instance._hub.inventory.items[itemIndex].durability <= 0f)
				{
					return false;
				}

				if (Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, sourcePos) > 5.5f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.6 (difference between real source position and provided source position is too big)", "gray");
					return false;
				}

				if (sourcePos.y - __instance._hub.playerMovementSync.LastSafePosition.y > 1.78f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.7 (Y axis difference between last safe position and provided source position is too big)", "gray");
					return false;
				}

				if (Math.Abs(sourcePos.y - __instance._hub.playerMovementSync.RealModelPosition.y) > 2.7f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.8 (|Y| axis difference between real position and provided source position is too big)", "gray");
					return false;
				}

				Player myPlayer = PlayersList.GetPlayer(__instance._hub);

				WeaponShootEvent shootEvent = new WeaponShootEvent(myPlayer, target, __instance._hub.inventory.items[itemIndex],
					__instance._hub.weaponManager.weapons[__instance.curWeapon], dir, sourcePos, targetPos, hitboxType, __instance.curWeapon, itemIndex, true);
				CustomWeapon.EventHandler.OnShoot(shootEvent);
				EventManager.Trigger<IHandlerWeaponShoot>(shootEvent);

				if (!shootEvent.Allow)
                {
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, $"Shot rejected - disallowed by a server modification.", "gray");
					return false;
                }

				__instance._hub.inventory.items.ModifyDuration(itemIndex, __instance._hub.inventory.items[itemIndex].durability - 1f);

				if (!PluginManager.Config.ShouldKeepScp268)
					__instance.scp268.ServerDisable();

				__instance._fireCooldown = 1f / (__instance.weapons[__instance.curWeapon].shotsPerSecond * __instance.weapons[__instance.curWeapon].allEffects.firerateMultiplier) * 0.9f;

				float num = __instance.weapons[__instance.curWeapon].allEffects.audioSourceRangeScale;
				num = num * num * 70f;

				__instance.GetComponent<Scp939_VisionController>().MakeNoise(Mathf.Clamp(num, 5f, 100f));

				bool flag = target != null;

				RaycastHit raycastHit2;

				if (targetPos == Vector3.zero)
				{
					RaycastHit raycastHit;

					if (Physics.Raycast(sourcePos, dir, out raycastHit, 500f, __instance.raycastMask))
					{
						HitboxIdentity component = raycastHit.collider.GetComponent<HitboxIdentity>();

						if (component != null)
						{
							WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
							if (componentInParent != null)
							{
								flag = false;
								target = componentInParent.gameObject;
								hitboxType = component.id;
								targetPos = componentInParent.transform.position;
							}
						}
					}
				}
				else if (Physics.Linecast(sourcePos, targetPos, out raycastHit2, __instance.raycastMask))
				{
					HitboxIdentity component2 = raycastHit2.collider.GetComponent<HitboxIdentity>();

					if (component2 != null)
					{
						WeaponManager componentInParent2 = component2.GetComponentInParent<WeaponManager>();

						if (componentInParent2 != null)
						{
							if (componentInParent2.gameObject == target)
							{
								flag = false;
							}
							else if (componentInParent2.scp268.Enabled)
							{
								flag = false;
								target = componentInParent2.gameObject;
								hitboxType = component2.id;
								targetPos = componentInParent2.transform.position;
							}
						}
					}
				}

				Player myTarget = null;

				if (target != null)
					myTarget = PlayersList.GetPlayer(target);

				if (myTarget != null && __instance.GetShootPermission(myTarget.ReferenceHub.characterClassManager, false))
				{
					if (Math.Abs(__instance._hub.playerMovementSync.RealModelPosition.y - myTarget.ReferenceHub.playerMovementSync.RealModelPosition.y) > 35f)
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.1 (too big Y-axis difference between source and target)", "gray");
						return false;
					}

					if (Vector3.Distance(myTarget.ReferenceHub.playerMovementSync.RealModelPosition, targetPos) > 5f)
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.2 (difference between real target position and provided target position is too big)", "gray");
						return false;
					}

					if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, sourcePos, __instance.raycastServerMask))
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.3 (collision between source positions detected)", "gray");
						return false;
					}

					if (flag && Physics.Linecast(sourcePos, targetPos, __instance.raycastServerMask))
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.4 (collision on shot line detected)", "gray");
						return false;
					}

					if (myPlayer == myTarget)
					{
						__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.5 (target is itself)", "gray");
						return false;
					}

					Vector3 vector = myTarget.ReferenceHub.playerMovementSync.RealModelPosition - __instance._hub.playerMovementSync.RealModelPosition;

					float sqrMagnitude = vector.sqrMagnitude;

					if (Math.Abs(vector.y) < 10f && sqrMagnitude > 7.84f && (myTarget.ReferenceHub.characterClassManager.CurClass != RoleType.Scp0492 || sqrMagnitude > 9f) 
						&& ((myTarget.ReferenceHub.characterClassManager.CurClass != RoleType.Scp93953 && myTarget.ReferenceHub.characterClassManager.CurClass != RoleType.Scp93989) 
						|| sqrMagnitude > 18.49f))
					{
						float num2 = Math.Abs(Misc.AngleIgnoreY(vector, __instance.transform.forward));

						if (num2 > 45f)
						{
							__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.12 (too big angle)", "gray");
							return false;
						}

						if (__instance._lastAngleReset > 0f && num2 > 25f && Math.Abs(Misc.AngleIgnoreY(vector, __instance._lastAngle)) > 60f)
						{
							__instance._lastAngle = vector;
							__instance._lastAngleReset = 0.4f;
							__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.13 (too big angle v2)", "gray");

							return false;
						}

						__instance._lastAngle = vector;
						__instance._lastAngleReset = 0.4f;
					}

					if (__instance._lastRotationReset > 0f && (__instance._hub.playerMovementSync.Rotations.x < 68f || __instance._hub.playerMovementSync.Rotations.x > 295f))
					{
						float num3 = __instance._hub.playerMovementSync.Rotations.x - __instance._lastRotation;

						if (num3 >= 0f && num3 <= 0.0005f)
						{
							__instance._lastRotation = __instance._hub.playerMovementSync.Rotations.x;
							__instance._lastRotationReset = 0.35f;
							__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.9 (no recoil)", "gray");

							return false;
						}
					}

					__instance._lastRotation = __instance._hub.playerMovementSync.Rotations.x;
					__instance._lastRotationReset = 0.35f;

					float num4 = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
					float num5 = __instance.weapons[__instance.curWeapon].damageOverDistance.Evaluate(num4);

					RoleType curClass = myTarget.ReferenceHub.characterClassManager.CurClass;

					if (curClass != RoleType.Scp173)
					{
						switch (curClass)
						{
							case RoleType.Scp106:
								num5 /= 10f;
								goto IL_81C;
							case RoleType.NtfScientist:
							case RoleType.Scientist:
							case RoleType.ChaosInsurgency:
								break;
							case RoleType.Scp049:
							case RoleType.Scp079:
							case RoleType.Scp096:
								goto IL_81C;
							default:
								if (curClass - RoleType.Scp93953 <= 1)
									goto IL_81C;
								break;
						}

						if (hitboxType > HitBoxType.ARM)
						{
							if (hitboxType == HitBoxType.HEAD)
							{
								num5 *= 4f;
								float num6 = 1f / (__instance.weapons[__instance.curWeapon].shotsPerSecond * __instance.weapons[__instance.curWeapon].allEffects.firerateMultiplier);

								__instance._headshotsL += 1U;
								__instance._headshotsS += 1U;
								__instance._headshotsResetS = num6 * 1.86f;
								__instance._headshotsResetL = num6 * 2.9f;

								if (__instance._headshotsS >= 3U)
								{
									__instance._hub.playerMovementSync.AntiCheatKillPlayer("Headshots limit exceeded in time window A\n(debug code: W.10)", "W.10");
									return false;
								}

								if (__instance._headshotsL >= 4U)
								{
									__instance._hub.playerMovementSync.AntiCheatKillPlayer("Headshots limit exceeded in time window B\n(debug code: W.11)", "W.11");
									return false;
								}
							}
						}
						else
						{
							num5 /= 2f;
						}
					}
				IL_81C:
					num5 *= __instance.weapons[__instance.curWeapon].allEffects.damageMultiplier;
					num5 *= __instance.overallDamagerFactor;

					WeaponLateShootEvent lateEv = new WeaponLateShootEvent(myPlayer, myTarget, num5, true);
					EventManager.Trigger<IHandlerWeaponLateShoot>(lateEv);

					if (!lateEv.Allow)
						return false;

					__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(lateEv.Damage, __instance._hub.LoggedNameFromRefHub(), 
						DamageTypes.FromWeaponId(__instance.curWeapon), __instance._hub.queryProcessor.PlayerId), 
						myTarget.ReferenceHub.gameObject, false, true);

					__instance.RpcConfirmShot(true, __instance.curWeapon);
					__instance.PlaceDecal(true, new Ray(__instance.camera.position, dir), (int)myTarget.ReferenceHub.characterClassManager.CurClass, num4);

					return false;
				}
				else
				{
					BreakableWindow bw = null;

					if (target != null && hitboxType == HitBoxType.WINDOW && ((bw = target.GetComponent<BreakableWindow>()) != null))
					{
						float time = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
						float damage = __instance.weapons[__instance.curWeapon].damageOverDistance.Evaluate(time);

						Window window = bw.GetWindow();

						if (window != null)
                        {
							DamageWindowEvent damageWindowEvent = new DamageWindowEvent(myPlayer, window,
								DamageTypes.FromWeaponId(__instance.curWeapon).AsDamageType(), damage, true);
							EventManager.Trigger<IHandlerDamageWindow>(damageWindowEvent);

							if (!damageWindowEvent.Allow)
								return false;
                        }

						bw.ServerDamageWindow(damage);

						__instance.RpcConfirmShot(true, __instance.curWeapon);

						return false;
					}

					__instance.PlaceDecal(false, new Ray(__instance.camera.position, dir), __instance.curWeapon, 0f);
					__instance.RpcConfirmShot(false, __instance.curWeapon);

					return false;
				}
			}
            catch (Exception e)
            {
                Patcher.Log(typeof(WeaponManager_CallCmdShoot), e);
                return true;
            }
        }
    }
}
