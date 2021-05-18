using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Custom.Items.API;
using UnityEngine;
using Harmony;
using NorthwoodLib.Pools;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    public static class CharacterClassManager_SetPlayersClass
    {
        public static bool Prefix(CharacterClassManager __instance, ref RoleType classid, GameObject ply, bool lite = false, bool escape = false)
        {
            try
            {
				ReferenceHub hub = ReferenceHub.GetHub(ply);

				if (hub == null || hub.isDedicatedServer || !hub.Ready)
					return false;

				Player player = PlayersList.GetPlayer(hub);

				if (player != null)
				{
					SetClassEvent ev = new SetClassEvent(player, classid, true);
					CustomItem.EventHandling.OnSetClass(ev);
					EventManager.Trigger<IHandlerSetClass>(ev);

					if (!ev.Allow)
						return false;
				}

				hub.characterClassManager.SetClassIDAdv(classid, lite, escape);
				hub.fpc.ResetStamina();
				hub.playerStats.SetHPAmount(__instance.Classes.SafeGet(classid).maxHP);

				if (lite)
					return false;

				Inventory inventory = hub.inventory;
				List<Inventory.SyncItemInfo> list = ListPool<Inventory.SyncItemInfo>.Shared.Rent();

				if (escape && CharacterClassManager.KeepItemsAfterEscaping)
				{
					foreach (Inventory.SyncItemInfo item in inventory.items)
					{
						list.Add(item);
					}
				}

				inventory.items.Clear();

				foreach (ItemType id in __instance.Classes.SafeGet(classid).startItems)
				{
					inventory.AddNewItem(id, -4.6566467E+11f, 0, 0, 0);
				}

				if (escape && CharacterClassManager.KeepItemsAfterEscaping)
				{
					foreach (Inventory.SyncItemInfo syncItemInfo in list)
					{
						if (CharacterClassManager.PutItemsInInvAfterEscaping)
						{
							Item itemByID = inventory.GetItemByID(syncItemInfo.id);
							bool flag = false;
							InventoryCategory[] categories = __instance._search.categories;
							int i = 0;
							while (i < categories.Length)
							{
								InventoryCategory inventoryCategory = categories[i];
								if (inventoryCategory.itemType == itemByID.itemCategory && itemByID.itemCategory != ItemCategory.None)
								{
									int num = 0;
									foreach (Inventory.SyncItemInfo syncItemInfo2 in inventory.items)
									{
										if (inventory.GetItemByID(syncItemInfo2.id).itemCategory == itemByID.itemCategory)
										{
											num++;
										}
									}
									if (num >= inventoryCategory.maxItems)
									{
										flag = true;
										break;
									}
									break;
								}
								else
								{
									i++;
								}
							}
							if (inventory.items.Count >= 8 || flag)
							{
								inventory.SetPickup(syncItemInfo.id, syncItemInfo.durability, __instance._pms.RealModelPosition, Quaternion.Euler(__instance._pms.Rotations.x, __instance._pms.Rotations.y, 0f), syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther, true);
							}
							else
							{
								inventory.AddNewItem(syncItemInfo.id, syncItemInfo.durability, syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther);
							}
						}
						else
						{
							inventory.SetPickup(syncItemInfo.id, syncItemInfo.durability, __instance._pms.RealModelPosition, Quaternion.Euler(__instance._pms.Rotations.x, __instance._pms.Rotations.y, 0f), syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther, true);
						}
					}
				}

				ListPool<Inventory.SyncItemInfo>.Shared.Return(list);

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(CharacterClassManager_SetPlayersClass), e);
                return true;
            }
        }
    }
}
