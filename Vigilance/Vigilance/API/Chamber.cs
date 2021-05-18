using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public class Chamber
    {
        private LockerChamber _chamberId;

        public Chamber(LockerChamber chamber, byte chamberId)
        {
            _chamberId = chamber;
            ChamberId = chamberId;
        }

        public byte ChamberId { get; }
        public List<string> CustomPermissions { get; } = new List<string>();
        public string AccessToken { get => _chamberId.accessToken; set => _chamberId.accessToken = value; }
        public string ItemTag { get => _chamberId.itemTag; set => _chamberId.itemTag = value; }
        public bool Used { get => _chamberId.used; set => _chamberId.used = value; }
        public bool IsVirtual { get => _chamberId.Virtual; set => _chamberId.Virtual = value; }
        public bool IsFree => _chamberId.IsFree();
        public string Name => _chamberId.name;
        public float LastOpened => _chamberId.lastOpened;
        public Vector3 Spawnpoint => _chamberId.spawnpoint.position;

        public void SetCooldown() => _chamberId.SetCooldown();

        public bool CanUse(Player player)
        {
            if (player.BypassMode)
                return true;

            if (player.PlayerLock)
                return false;

            if (string.IsNullOrEmpty(AccessToken))
                return true;

            if (PluginManager.Config.LockerPermissions.Contains("SCP_ACCESS") && player.IsSCP)
                return true;

            if (PluginManager.Config.RemoteCard)
            {
                foreach (Inventory.SyncItemInfo itemInfo in player.ReferenceHub.inventory.items)
                {
                    if (!itemInfo.id.IsKeycard())
                        continue;

                    foreach (string perm in player.ReferenceHub.inventory.GetItemByID(itemInfo.id).permissions)
                    {
                        if (PluginManager.Config.LockerPermissions.Contains(perm))
                            return true;
                    }
                }
            }
            else
            {
                Item inhand = player.ReferenceHub.inventory.GetItemByID(player.ReferenceHub.inventory.curItem);

                if (inhand != null && inhand.id.IsKeycard())
                {
                    foreach (string perm in inhand.permissions)
                    {
                        if (PluginManager.Config.LockerPermissions.Contains(perm))
                            return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public void ChangePickupLocation(Vector3 pos)
        {
            NetworkServer.UnSpawn(_chamberId.spawnpoint.gameObject);
            _chamberId.spawnpoint.position = pos;
            NetworkServer.Spawn(_chamberId.spawnpoint.gameObject);
        }
    }
}
