using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using UnityEngine;
using Harmony;
using Mirror;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.SetPickup))]
    public static class Inventory_SetPickup
    {
        public static bool Prefix(Inventory __instance, ItemType droppedItemId, float dur, Vector3 pos, Quaternion rot, int s, int b, int o, ref Pickup __result, bool spawnAutomatically = true)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return true;

                Pickup pickup = UnityEngine.Object.Instantiate(__instance.pickupPrefab).GetComponent<Pickup>();
                Player owner = PlayersList.GetPlayer(__instance._hub);

                if (owner == null)
                    owner = CompCache.Player;

                PatchData.AddPickup(owner, pickup);

                SpawnItemEvent ev = new SpawnItemEvent(pickup, droppedItemId, dur, owner, new Pickup.WeaponModifiers(true, s, b, o), pos, rot, true);
                EventManager.Trigger<IHandlerSpawnItem>(ev);

                if (!ev.Allow)
                    return false;

                pickup.SetupPickup(ev.ItemId, ev.Durability, ev.Owner.GameObject, ev.WeaponModifiers, ev.Position, ev.Rotation);
                __result = pickup;

                if (spawnAutomatically)
                    NetworkServer.Spawn(pickup.gameObject);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Inventory_SetPickup), e);
                return true;
            }
        }
    }
}