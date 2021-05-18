using UnityEngine;
using Mirror;
using Vigilance.Utilities;
using Vigilance.Extensions;
using System.Collections.Generic;

namespace Vigilance.API
{
    public class Generator
    {
        private Generator079 _gen;

        public Generator(Generator079 gen)
        {
            _gen = gen;
            Room = GetRoom();
            DisallowedPlayers = new List<Player>();
        }

        public Room Room { get; }
        public List<Player> DisallowedPlayers { get; }
        public GameObject GameObject => _gen.gameObject;

        public Vector3 Position { get => _gen.transform.position; set => SetPosition(value); }
        public Vector3 Scale { get => _gen.transform.localScale; set => SetScale(value); }
        public Quaternion Rotation { get => _gen.transform.rotation; set => SetRotation(value); }

        public bool ForcedOvercharge => _gen.forcedOvercharge;
        public bool IsTabletConnected { get => _gen.NetworkisTabletConnected; set => _gen.NetworkisTabletConnected = value; }
        public bool IsDoorOpen { get => _gen.NetworkisDoorOpen; set => _gen.NetworkisDoorOpen = value; }
        public bool IsUnlocked { get => _gen.NetworkisDoorUnlocked; set => _gen.NetworkisDoorUnlocked = value; }
        public float RemainingPowerup { get => _gen.NetworkremainingPowerup; set => _gen.NetworkremainingPowerup = value; }
        public float LocalVoltage { get => _gen.localVoltage; set => _gen.localVoltage = value; }
        public byte ActivatedGenerators { get => _gen.NetworktotalVoltage; set => _gen.NetworktotalVoltage = value; }

        public void Eject()
        {
            if (!IsTabletConnected)
                return;

            _gen.EjectTablet();
        }

        public void Insert(Player player = null)
        {
            if (IsTabletConnected)
                return;

            if (player == null)
                CompCache.Player.AddItem(ItemType.WeaponManagerTablet);

            _gen.Interact(player == null ? CompCache.Player.GameObject : player.GameObject, PlayerInteract.Generator079Operations.Tablet);
        }

        public void Interact(Player player, PlayerInteract.Generator079Operations operation) => _gen.Interact(player == null ? CompCache.Player.GameObject : player.GameObject, operation);

        public void Overcharge(float time, bool onlyHeavy = false)
        {
            _gen.ServerOvercharge(time, onlyHeavy);
        }

        public void Lock()
        {
            IsUnlocked = false;
        }

        public void Unlock()
        {
            _gen.Unlock();
            IsUnlocked = true;
        }

        public void Open()
        {
            IsDoorOpen = true;
        }

        public void Close()
        {
            IsDoorOpen = false;
        }

        public bool CanUnlock(Player player)
        {
            if (player.BypassMode)
                return true;

            if (player.PlayerLock)
                return false;

            if (DisallowedPlayers.Contains(player))
                return false;

            if (PluginManager.Config.GeneratorPermissions.Contains("SCP_ACCESS") && player.IsSCP)
                return true;

            if (PluginManager.Config.RemoteCard)
            {
                foreach (Inventory.SyncItemInfo itemInfo in player.ReferenceHub.inventory.items)
                {
                    if (!itemInfo.id.IsKeycard())
                        continue;

                    foreach (string perm in player.ReferenceHub.inventory.GetItemByID(itemInfo.id).permissions)
                    {
                        if (PluginManager.Config.GeneratorPermissions.Contains(perm))
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
                        if (PluginManager.Config.GeneratorPermissions.Contains(perm))
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

        private Room GetRoom() => _gen.gameObject.AddComponent<Room>();

        private void SetPosition(Vector3 pos)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.position = pos;
            NetworkServer.Spawn(GameObject);
        }

        private void SetRotation(Quaternion rot)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.rotation = rot;
            NetworkServer.Spawn(GameObject);
        }

        private void SetScale(Vector3 scale)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.localScale = scale;
            NetworkServer.Spawn(GameObject);
        }
    }
}
