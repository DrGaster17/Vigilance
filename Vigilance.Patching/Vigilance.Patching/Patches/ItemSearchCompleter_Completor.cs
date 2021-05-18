using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Searching;
using Hints;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
    public static class ItemSearchCompleter_Completor
    {
        public static bool Prefix(ItemSearchCompletor __instance)
        {
            try
            {
				Player player = PlayersList.GetPlayer(__instance.Hub);

				// This shouldn't happen but just in case
				if (player == null)
					return true;

				if (player.PlayerLock)
					return false;

				PickupItemEvent ev = new PickupItemEvent(__instance.TargetPickup, player, true);
				EventManager.Trigger<IHandlerPickupItem>(ev);

				if (!ev.Allow)
				{
					__instance.TargetPickup.InUse = false;
					return false;
				}

				if (__instance.TargetPickup.weaponMods.Present)
				{
					Pickup.WeaponModifiers weaponModifiers = new Pickup.WeaponModifiers(false, 0, 0, 0);

					foreach (WeaponManager.Weapon weapon in __instance.Hub.weaponManager.weapons)
					{
						if (weapon.inventoryID == __instance.TargetPickup.itemId)
						{
							weaponModifiers = new Pickup.WeaponModifiers(true, Mathf.Clamp(__instance.TargetPickup.weaponMods.Sight, 0, weapon.mod_sights.Length - 1), Mathf.Clamp(__instance.TargetPickup.weaponMods.Barrel, 0, weapon.mod_barrels.Length - 1), Mathf.Clamp(__instance.TargetPickup.weaponMods.Other, 0, weapon.mod_others.Length - 1));
						}
					}

					__instance.Hub.inventory.AddNewItem(__instance.TargetPickup.itemId, __instance.TargetPickup.durability, weaponModifiers.Sight, weaponModifiers.Barrel, weaponModifiers.Other);
				}
				else
				{
					__instance.Hub.inventory.AddNewItem(__instance.TargetPickup.itemId, __instance.Hub.inventory.GetItemByID(__instance.TargetPickup.itemId).durability, 0, 0, 0);
				}

				__instance.TargetPickup.Delete();

				if (__instance._category != null && !__instance._category.hideWarning && __instance.CategoryCount >= __instance._category.maxItems)
				{
					__instance.Hub.hints.Show(new TranslationHint(HintTranslations.MaxItemCategoryReached, new HintParameter[]
					{
						new ItemCategoryHintParameter(__instance._category.itemType),
						new ByteHintParameter(__instance._category.maxItems)
					}, HintEffectPresets.FadeInAndOut(0.25f, 1f, 0f), 1.5f));
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ItemSearchCompleter_Completor), e);
                return true;
            }
        }
    }
}
