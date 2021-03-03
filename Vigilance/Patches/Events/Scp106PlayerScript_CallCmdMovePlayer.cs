using System;
using Harmony;
using Vigilance.API;
using Vigilance.Utilities;
using UnityEngine;
using CustomPlayerEffects;
using RemoteAdmin;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    public static class Scp106PlayerScript_CallCmdMovePlayer
    {
        public static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
				if (!__instance._iawRateLimit.CanExecute(true) || !__instance.iAm106 || !ServerTime.CheckSynchronization(t)) return false;
				if (ply == null) return false;
				Player player = Server.PlayerList.GetPlayer(ply);
				if (player == null) return true;
				ReferenceHub hub = player.Hub;
				CharacterClassManager characterClassManager = hub.characterClassManager;
				if (characterClassManager == null || characterClassManager.GodMode || !characterClassManager.IsHuman()) return false;
				Vector3 position = ply.transform.position;
				float num = Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, position);
				float num2 = Math.Abs(__instance._hub.playerMovementSync.RealModelPosition.y - position.y);

				if ((num >= 1.818f && num2 < 1.02f) || (num >= 2.1f && num2 < 1.95f) || (num >= 2.65f && num2 < 2.2f) || (num >= 3.2f && num2 < 3f) || num >= 3.64f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, string.Format("106 MovePlayer command rejected - too big distance (code: T1). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
					return false;
				}

				if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, ply.transform.position, __instance._hub.weaponManager.raycastServerMask))
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, string.Format("106 MovePlayer command rejected - collider found between you and the target (code: T2). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
					return false;
				}

				Handling.OnPocketEnter(player, true, ConfigManager.Scp106PocketEnterDamage, true, out bool hurt, out float damage, out bool allow);
				if (!allow) return false;
				__instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
				__instance.TargetHitMarker(__instance.connectionToClient, __instance.captureCooldown);
				__instance._currentServerCooldown = __instance.captureCooldown;
				if (Scp106PlayerScript._blastDoor.isClosed)
				{
					__instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
					if (hurt) __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(500f, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply, false, true);
				}
				else
				{
					if (hurt) __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(damage, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply, false, true);
					hub.playerMovementSync.OverridePosition(Vector3.down * 1998.5f, 0f, true);
					foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					{
						Scp079Interactable.ZoneAndRoom otherRoom = ply.GetComponent<Scp079PlayerScript>().GetOtherRoom();
						Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
						{
							Scp079Interactable.InteractableType.Door,
							Scp079Interactable.InteractableType.Light,
							Scp079Interactable.InteractableType.Lockdown,
							Scp079Interactable.InteractableType.Tesla,
							Scp079Interactable.InteractableType.ElevatorUse
						};

						bool flag = false;
						foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, filter))
						{
							foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
							{
								if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
								{
									flag = true;
								}
							}
						}

						if (flag)
						{
							scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, characterClassManager.CurClass);
						}
					}
				}

				PlayerEffectsController playerEffectsController = hub.playerEffectsController;
				playerEffectsController.GetEffect<Corroding>().IsInPd = true;
				playerEffectsController.EnableEffect<Corroding>(0f, false);
				return false;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp106PlayerScript.CallCmdMovePlayer), e);
                return true;
            }
        }
    }
}
