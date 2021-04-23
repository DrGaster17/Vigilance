using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Extensions;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseLocker))]
    public static class PlayerInteract_CallCmdUseLocker
    {
        public static bool Prefix(PlayerInteract __instance, byte lockerId, byte chamberNumber)
        {
            try
            {
				if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 || (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract))
					return false;

				LockerManager singleton = LockerManager.singleton;

				if (lockerId >= singleton.lockers.Length || !__instance.ChckDis(singleton.lockers[lockerId].gameObject.position) || !singleton.lockers[lockerId].supportsStandarizedAnimation
					|| chamberNumber >= singleton.lockers[lockerId].chambers.Length || singleton.lockers[lockerId].chambers[chamberNumber].doorAnimator == null
					|| !singleton.lockers[lockerId].chambers[chamberNumber].CooldownAtZero())
					return false;

				Player player = PlayersList.GetPlayer(__instance._hub);
				API.Locker locker = singleton.lockers[lockerId].GetLocker();

				if (locker == null || player == null)
					return true;

				if (player.PlayerLock)
					return false;

				Chamber chamber = locker.GetChamber(chamberNumber);

				if (chamber == null)
					return true;

				chamber.SetCooldown();

				List<string> customTokens = chamber.CustomPermissions;
				string token = chamber.AccessToken;
				Item itemByID = __instance._inv.GetItemByID(__instance._inv.curItem);
				bool allow = chamber.CanUse(player);

				LockerInteractEvent ev = new LockerInteractEvent(locker, chamber, player, allow);
				EventManager.Trigger<IHandlerLockerInteract>(ev);

				if (ev.Allow)
				{
					PlayerUseLockerEvent useEv = new PlayerUseLockerEvent(player, singleton.lockers[lockerId], token, true);
					EventManager.Trigger<IHandlerPlayerUseLocker>(useEv);

					if (!useEv.Allow)
						return false;

					bool flag = (singleton.openLockers[lockerId] & 1 << chamberNumber) != 1 << chamberNumber;
					singleton.ModifyOpen(lockerId, chamberNumber, flag);
					singleton.RpcDoSound(lockerId, chamberNumber, flag);
					bool anyOpen = singleton.lockers[lockerId].AnyVirtual;

					if (singleton.lockers[lockerId].AnyVirtual)
					{
						for (int i = 0; i < singleton.lockers[lockerId].chambers.Length; i++)
						{
							if ((singleton.openLockers[lockerId] & 1 << i) == 1 << i)
							{
								anyOpen = false;
								break;
							}
						}
					}

					singleton.lockers[lockerId].LockPickups(!flag, chamberNumber, anyOpen);

					if (!string.IsNullOrEmpty(token))
					{
						singleton.RpcChangeMaterial(lockerId, chamberNumber, false);
					}
				}
				else
				{
					singleton.RpcChangeMaterial(lockerId, chamberNumber, true);
				}

				__instance.OnInteract();

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdUseLocker), e);
                return true;
            }
        }
    }
}
