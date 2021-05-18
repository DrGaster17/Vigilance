using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
	[HarmonyPatch(typeof(Generator079), nameof(Generator079.Interact))]
    public static class Generator079_Interact
    {
        public static bool Prefix(Generator079 __instance, GameObject person, PlayerInteract.Generator079Operations command)
        {
			try
			{
				Player player = PlayersList.GetPlayer(person);
				Generator generator = __instance.GetGenerator();

				if (player == null || generator == null)
					return true;

				if (player.PlayerLock || generator.DisallowedPlayers.Contains(player))
					return false;

				switch (command)
				{
					case PlayerInteract.Generator079Operations.Door:
						__instance.OpenClose(person);
						return false;

					case PlayerInteract.Generator079Operations.Tablet:
						{
							if (__instance.isTabletConnected || !__instance.isDoorOpen || __instance._localTime <= 0f || Generator079.mainGenerator.forcedOvercharge)
								return false;

							Inventory.SyncListItemInfo items = player.ReferenceHub.inventory.items;

							for (int i = 0; i < items.Count; i++)
                            {
								if (items[i].id == ItemType.WeaponManagerTablet)
                                {
									GeneratorInsertEvent ev = new GeneratorInsertEvent(generator, player, true);
									EventManager.Trigger<IHandlerGeneratorInsert>(ev);

									if (!ev.Allow)
										return false;

									player.RemoveItem(items[i]);

									__instance.NetworkisTabletConnected = true;

									return false;
                                }
                            }

							return false;
						}

					case PlayerInteract.Generator079Operations.Cancel:
						return false;

					default:
						break;
				}

				GeneratorEjectEvent eJev = new GeneratorEjectEvent(generator, player, true);
				EventManager.Trigger<IHandlerGeneratorEject>(eJev);

				if (!eJev.Allow)
					return false;

				__instance.EjectTablet();
				return false;
			}
			catch (Exception e)
            {
				Patcher.Log(typeof(Generator079_Interact), e);
				return true;
            }
		}
    }
}
