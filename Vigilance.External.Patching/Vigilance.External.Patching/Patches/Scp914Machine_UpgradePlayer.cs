using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Scp914;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradePlayer))]
    public static class Scp914Machine_UpgradePlayer
    {
        public static bool Prefix(Scp914Machine __instance, Inventory inventory, CharacterClassManager player, IEnumerable<CharacterClassManager> players)
        {
            try
            {
                if (player == null)
                    return false;

                Player ply = PlayersList.GetPlayer(player._hub);

                if (ply == null)
                    return true;

                Scp914UpgradePlayerEvent ev = new Scp914UpgradePlayerEvent(ply, true);
                EventManager.Trigger<IHandlerScp914UpgradePlayer>(ev);

                if (!ev.Allow)
                    return false;

                for (int i = inventory.items.Count - 1; i > -1; i--)
                {
                    Inventory.SyncItemInfo syncItemInfo = inventory.items[i];

                    ItemType itemType = __instance.UpgradeItemID(syncItemInfo.id);

                    if (itemType < ItemType.KeycardJanitor)
                    {
                        inventory.items.RemoveAt(i);
                    }
                    else
                    {
                        syncItemInfo.id = itemType;

                        inventory.items[i] = syncItemInfo;

                        Scp914Machine.TryFriendshipAchievement(itemType, player, players);
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp914Machine_UpgradePlayer), e);
                return true;
            }
        }
    }
}
