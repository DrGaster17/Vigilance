using System;
using System.Collections.Generic;
using System.Linq;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Interactables.Interobjects.DoorUtils;
using Scp914;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeObjects))]
    public static class Scp914Machine_UpgradeObjects
    {
        public static bool Prefix(Scp914Machine __instance, IEnumerable<Pickup> items, IReadOnlyCollection<CharacterClassManager> players)
        {
            try
            {
				SCP914UpgradeEvent ev = new SCP914UpgradeEvent(__instance, players.Select(x => x.gameObject), items, __instance.NetworkknobState, true);
				EventManager.Trigger<IHandlerScp914Upgrade>(ev);
				Custom.Items.API.CustomItem.EventHandling.OnUpgrade(ev);

				if (!ev.Allow)
					return false;

				if (ev.KnobSetting != __instance.NetworkknobState)
					__instance.NetworkknobState = ev.KnobSetting;

				if (__instance.configMode.Value.HasFlagFast(Scp914Mode.Dropped))
				{
					foreach (Pickup pickup in items)
					{
						__instance.UpgradeItem(pickup);
						Scp914Machine.TryFriendshipAchievement(pickup.ItemId, pickup.ownerPlayer.GetComponent<CharacterClassManager>(), players);
					}
				}

				if (!__instance.configMode.Value.HasFlagFast(Scp914Mode.Inventory))
					return false;

				foreach (CharacterClassManager ccm in players)
				{
					if (__instance.configMode.Value.HasFlagFast(Scp914Mode.Held))
						__instance.UpgradeHeldItem(ccm._hub.inventory, ccm, players);
					else
						__instance.UpgradePlayer(ccm._hub.inventory, ccm, players);
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp914Machine_UpgradeObjects), e);
                return true;
            }
        }
    }
}
