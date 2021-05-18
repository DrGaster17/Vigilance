using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using CustomPlayerEffects;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    public static class Scp106PlayerScript_CallCmdMovePlayer
    {
        public static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
				if (!__instance._iawRateLimit.CanExecute(true) || !__instance.iAm106 || !ServerTime.CheckSynchronization(t) || ply == null)
					return false;

				Player player = PlayersList.GetPlayer(ply);

				if (player == null)
					return true;

				if (player.GodMode && !player.ReferenceHub.characterClassManager.IsHuman())
					return false;

				if (player.PlayerLock)
					return false;

				Vector3 position = player.Position;
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


				PocketEnterEvent ev = new PocketEnterEvent(player, true, Scp106PlayerScript._blastDoor.isClosed ? 500f : PluginManager.Config.Scp106PocketEnterDamage, true);
				EventManager.Trigger<IHandlerPocketEnter>(ev);

				if (!ev.Allow)
					return false;

				__instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
				__instance.TargetHitMarker(__instance.connectionToClient, __instance.captureCooldown);
				__instance._currentServerCooldown = __instance.captureCooldown;

				if (Scp106PlayerScript._blastDoor.isClosed)
				{
					if (ev.Hurt)
					{
						__instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
						__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Damage, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance._hub.queryProcessor.PlayerId), ply, false, true);
					}
				}
				else
				{
					if (ev.Hurt)
						__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Damage, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance._hub.queryProcessor.PlayerId), ply, false, true);

					player.ReferenceHub.playerMovementSync.OverridePosition(Vector3.down * 1998.5f, 0f, true);

					Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
					{
							Scp079Interactable.InteractableType.Door,
							Scp079Interactable.InteractableType.Light,
							Scp079Interactable.InteractableType.Lockdown,
							Scp079Interactable.InteractableType.Tesla,
							Scp079Interactable.InteractableType.ElevatorUse
					};

					foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					{
						Scp079Interactable.ZoneAndRoom otherRoom = player.ReferenceHub.scp079PlayerScript.GetOtherRoom();

						foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, filter))
						{
							foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
							{
								if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
								{
									scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, player.Role);
								}
							}
						}
					}
				}

				player.GetEffect<Corroding>().IsInPd = true;
				player.EnableEffect<Corroding>();

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp106PlayerScript_CallCmdMovePlayer), e);
                return true;
            }
        }
    }
}
