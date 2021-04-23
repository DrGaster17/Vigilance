using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Custom.Items.API;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdReload))]
    public static class WeaponManager_CallCmdReload
    {
        public static bool Prefix(WeaponManager __instance, bool animationOnly)
        {
            try
            {
				if (!__instance._iawRateLimit.CanExecute(true))
					return false;

				int itemIndex = __instance._hub.inventory.GetItemIndex();

				if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count)
					return false;

				if (__instance.curWeapon < 0 || __instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID)
					return false;

				if (__instance._hub.inventory.items[itemIndex].durability >= __instance.weapons[__instance.curWeapon].maxAmmo)
					return false;

				Player player = PlayersList.GetPlayer(__instance._hub);

				if (player == null)
					return true;

				WeaponReloadEvent ev = new WeaponReloadEvent(player, animationOnly, true);
				CustomWeapon.EventHandler.OnReload(ev);
				EventManager.Trigger<IHandlerWeaponReload>(ev);

				if (!ev.Allow)
					return false;

				animationOnly = ev.AnimationOnly;

				if (!PluginManager.Config.ShouldKeepScp268)
					__instance.scp268.ServerDisable();

				if (animationOnly)
				{
					__instance.RpcReload(__instance.curWeapon);
					__instance._reloadingWeapon = __instance.curWeapon;

					if (__instance.weapons[__instance._reloadingWeapon].reloadingTime > __instance._reloadCooldown)
					{
						__instance._reloadCooldown = __instance.weapons[__instance._reloadingWeapon].reloadingTime;
						return false;
					}
				}
				else
				{
					if (__instance.curWeapon == __instance._reloadingWeapon && __instance._reloadingWeapon != -100)
					{
						__instance._reloadingWeapon = -100;

						int ammoType = __instance.weapons[__instance.curWeapon].ammoType;
						uint num = (uint)__instance._hub.inventory.items[itemIndex].durability;
						uint num2 = Math.Min(__instance._hub.ammoBox[ammoType], __instance.weapons[__instance.curWeapon].maxAmmo - num);

						__instance._hub.inventory.items.ModifyDuration(itemIndex, num + num2);

						AmmoBox ammoBox = __instance._hub.ammoBox;
						int type = ammoType;
						ammoBox[type] -= num2;

						return false;
					}

					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "Reload rejected - Code 2.6 (not requested reload of that weapon)", "gray");
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(WeaponManager_CallCmdReload), e);
                return true;
            }
        }
    }
}
