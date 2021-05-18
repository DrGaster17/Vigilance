using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using Vigilance.API;
using YamlDotNet.Serialization;
using MEC;

namespace Vigilance.Custom.Items.API
{
    public abstract class CustomItem
    {
        private ItemType type;

        public static HashSet<CustomItem> Registered { get; } = new HashSet<CustomItem>();

        public abstract uint Id { get; set; }
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract SpawnProperties SpawnProperties { get; set; }

        [YamlIgnore]
        public virtual float Durability { get; set; }

        public virtual ItemType Type
        {
            get => type;
            set
            {
                if (!Enum.IsDefined(typeof(ItemType), value))
                    throw new ArgumentOutOfRangeException("Type", value, "Invalid Item type.");

                type = value;
            }
        }

        [YamlIgnore]
        public HashSet<int> InsideInventories { get; } = new HashSet<int>();

        [YamlIgnore]
        public HashSet<Pickup> Spawned { get; } = new HashSet<Pickup>();

        public static CustomItem Get(int id) => Registered?.FirstOrDefault(tempCustomItem => tempCustomItem.Id == id);
        public static CustomItem Get(string name) => Registered?.FirstOrDefault(tempCustomItem => tempCustomItem.Name == name);

        public static bool TryGet(int id, out CustomItem customItem)
        {
            customItem = Get(id);

            return customItem != null;
        }

        public static bool TryGet(string name, out CustomItem customItem)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            customItem = int.TryParse(name, out int id) ? Get(id) : Get(name);

            return customItem != null;
        }

        public static bool TryGet(Player player, out CustomItem customItem)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            customItem = Registered?.FirstOrDefault(tempCustomItem => tempCustomItem.Check(player.CurrentItem));

            return customItem != null;
        }

        public static bool TryGet(Player player, out IEnumerable<CustomItem> customItems)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            customItems = Registered?.Where(tempCustomItem => player.ReferenceHub.inventory.items.Any(item => tempCustomItem.Check(item)));

            return customItems?.Any() ?? false;
        }

        public static bool TryGet(Inventory.SyncItemInfo item, out CustomItem customItem)
        {
            customItem = Registered?.FirstOrDefault(tempCustomItem => tempCustomItem.InsideInventories.Contains(item.uniq));

            return customItem != null;
        }

        public static bool TryGet(Pickup pickup, out CustomItem customItem)
        {
            customItem = Registered?.FirstOrDefault(tempCustomItem => tempCustomItem.Spawned.Contains(pickup));

            return customItem != null;
        }

        public static bool TrySpawn(int id, Vector3 position)
        {
            if (!TryGet(id, out CustomItem item))
                return false;

            item.Spawn(position);

            return true;
        }

        public static bool TrySpawn(string name, Vector3 position)
        {
            if (!TryGet(name, out CustomItem item))
                return false;

            item.Spawn(position);

            return true;
        }


        public static bool TryGive(Player player, string name)
        {
            if (!TryGet(name, out CustomItem item))
                return false;

            item.Give(player);

            return true;
        }

        public static bool TryGive(Player player, int id)
        {
            if (!TryGet(id, out CustomItem item))
                return false;

            item.Give(player);

            return true;
        }

        public bool TryRegister()
        {
            if (!Registered.Contains(this))
            {
                if (Registered.Any(customItem => customItem.Id == Id))
                {
                    Log.Warn($"{Name} has tried to register with the same custom item ID as another item: {Id}. It will not be registered.");

                    return false;
                }

                Registered.Add(this);

                Log.Debug($"{Name} ({Id}) [{Type}] has been successfully registered.");

                return true;
            }

            Log.Warn($"Couldn't register {Name} ({Id}) [{Type}] as it already exists.");

            return false;
        }

        public bool TryUnregister()
        {
            if (!Registered.Remove(this))
            {
                Log.Warn($"Cannot unregister {Name} ({Id}) [{Type}], it hasn't been registered yet.");
                return false;
            }

            return true;
        }


        public virtual void Spawn(float x, float y, float z) => Spawn(new Vector3(x, y, z));
        public virtual void Spawn(float x, float y, float z, Inventory.SyncItemInfo item) => Spawn(new Vector3(x, y, z), item);
        public virtual void Spawn(Player player) => Spawn(player.Position);
        public virtual void Spawn(Player player, Inventory.SyncItemInfo item) => Spawn(player.Position, item);
        public virtual void Spawn(Vector3 position) => Spawned.Add(ItemExtensions.Spawn(Type, Durability, position));
        public virtual void Spawn(Vector3 position, Inventory.SyncItemInfo item) => Spawned.Add(ItemExtensions.Spawn(item, position));

        public virtual uint Spawn(IEnumerable<SpawnPoint> spawnPoints, uint limit)
        {
            uint spawned = 0;

            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                Log.Debug($"Attempting to spawn {Name} at {spawnPoint.Position}.");

                if (UnityEngine.Random.Range(1, 101) >= spawnPoint.Chance || (limit > 0 && spawned >= limit))
                    continue;

                spawned++;

                Spawn(spawnPoint.Position.ToVector3);

                Log.Debug($"Spawned {Name} at {spawnPoint.Position} ({spawnPoint.Name})");
            }

            return spawned;
        }

        public virtual void SpawnAll()
        {
            if (SpawnProperties == null)
                return;

            Spawn(SpawnProperties.StaticSpawnPoints, Math.Min(0, SpawnProperties.Limit - Spawn(SpawnProperties.DynamicSpawnPoints, SpawnProperties.Limit)));
        }

        public virtual void Give(Player player, Inventory.SyncItemInfo item)
        {
            player.ReferenceHub.inventory.items.Add(item);
            InsideInventories.Add(item.uniq);
        }

        public virtual void Give(Player player, Pickup pickup) => Give(player, new Inventory.SyncItemInfo()
        {
            durability = pickup.durability,
            id = pickup.itemId,
            modBarrel = pickup.weaponMods.Barrel,
            modSight = pickup.weaponMods.Sight,
            modOther = pickup.weaponMods.Other,
            uniq = ++Inventory._uniqId,
        });

        public virtual void Give(Player player) => Give(player, new Inventory.SyncItemInfo()
        {
            durability = Durability,
            id = Type,
            uniq = ++Inventory._uniqId,
        });


        public virtual bool Check(Pickup pickup) => Spawned.Contains(pickup);
        public virtual bool Check(Inventory.SyncItemInfo item) => InsideInventories.Contains(item.uniq);
        public override string ToString() => $"[{Name} ({Type}) | {Id}] {Description}";

        protected virtual void OnOwnerChangeRole(SetClassEvent ev)
        {
        }

        protected virtual void OnOwnerDie(PlayerDieEvent ev)
        {
        }

        protected virtual void OnOwnerEscape(CheckEscapeEvent ev)
        {
        }

        protected virtual void OnOwnerHandcuff(HandcuffEvent ev)
        {
        }

        protected virtual void OnDrop(DropItemEvent ev)
        {
        }

        protected virtual void OnPickup(PickupItemEvent ev)
        {
        }

        protected virtual void OnUpgrade(Scp914UpgradeItemEvent ev)
        {
        }

        protected virtual void OnWaitingForPlayers()
        {
            InsideInventories.Clear();
            Spawned.Clear();
        }

        public static class EventHandling 
        {
            public static void OnSetClass(SetClassEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in Registered)
                {
                    foreach (Inventory.SyncItemInfo item in ev.Player.ReferenceHub.inventory.items.ToList())
                    {
                        if (!_item.Check(item))
                            continue;

                        _item.OnOwnerChangeRole(ev);

                        _item.InsideInventories.Remove(item.uniq);

                        ev.Player.RemoveItem(item);

                        _item.Spawn(ev.Player, item);
                    }
                }
            }

            public static void OnPlayerDie(PlayerDieEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in Registered)
                {
                    foreach (Inventory.SyncItemInfo item in ev.Target.ReferenceHub.inventory.items.ToList())
                    {
                        if (!_item.Check(item))
                            continue;

                        _item.OnOwnerDie(ev);

                        if (!ev.Allow)
                            continue;

                        ev.Target.RemoveItem(item);

                        _item.InsideInventories.Remove(item.uniq);

                        _item.Spawn(ev.Target, item);
                    }
                }
            }

            public static void OnCheckEscape(CheckEscapeEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in Registered)
                {
                    foreach (Inventory.SyncItemInfo item in ev.Player.ReferenceHub.inventory.items)
                    {
                        if (!_item.Check(item))
                            continue;

                        _item.OnOwnerEscape(ev);

                        if (!ev.Allow)
                            continue;

                        ev.Player.RemoveItem(item);

                        _item.InsideInventories.Remove(item.uniq);

                        _item.Spawn(Map.GetRandomSpawnpoint(ev.NewRole), item);
                    }
                }
            }

            public static void OnHandcuff(HandcuffEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in Registered)
                {
                    foreach (Inventory.SyncItemInfo item in ev.Target.ReferenceHub.inventory.items.ToList())
                    {
                        if (!_item.Check(item))
                            continue;

                        _item.OnOwnerHandcuff(ev);

                        if (!ev.Allow)
                            continue;

                        ev.Target.RemoveItem(item);

                        _item.InsideInventories.Remove(item.uniq);

                        _item.Spawn(ev.Target, item);
                    }
                }
            }

            public static void OnDropItem(DropItemEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in CustomItem.Registered)
                {
                    if (!_item.Check(ev.Item))
                        continue;

                    _item.OnDrop(ev);

                    if (!ev.Allow)
                        continue;

                    ev.Allow = false;

                    _item.InsideInventories.Remove(ev.Item.uniq);

                    ev.Player.RemoveItem(ev.Item);

                    _item.Spawn(ev.Player, ev.Item);
                }
            }

            public static void OnPickupItem(PickupItemEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in Registered)
                {
                    if (!_item.Check(ev.Item) || ev.Player.ReferenceHub.inventory.items.Count >= 8)
                        continue;

                    _item.OnPickup(ev);

                    if (!ev.Allow)
                        continue;

                    ev.Allow = false;

                    _item.Give(ev.Player, ev.Item);
                    _item.Spawned.Remove(ev.Item);

                    ev.Item.Delete();
                }
            }

            public static void OnUpgrade(SCP914UpgradeEvent ev)
            {
                if (Registered.Count < 1)
                    return;

                foreach (CustomItem _item in Registered)
                {
                    foreach (Pickup pickup in ev.Items.ToList())
                    {
                        if (!_item.Check(pickup))
                            continue;

                        pickup.transform.position = ev.Scp914.output.position;
                    }

                    Dictionary<Player, List<Inventory.SyncItemInfo>> playerToItems = new Dictionary<Player, List<Inventory.SyncItemInfo>>();

                    foreach (Player player in ev.Players)
                    {
                        playerToItems.Add(player, new List<Inventory.SyncItemInfo>());

                        foreach (Inventory.SyncItemInfo item in player.ReferenceHub.inventory.items)
                        {
                            if (!_item.Check(item))
                                continue;

                            _item.OnUpgrade(new Scp914UpgradeItemEvent(item.id, ev.Allow));
                            playerToItems[player].Add(item);
                            player.ReferenceHub.inventory.items.Remove(item);
                        }
                    }

                    Timing.CallDelayed(3.5f, () =>
                    {
                        foreach (KeyValuePair<Player, List<Inventory.SyncItemInfo>> playerToItemsPair in playerToItems)
                        {
                            foreach (Inventory.SyncItemInfo item in playerToItemsPair.Value)
                            {
                                if (playerToItemsPair.Key.ReferenceHub.inventory.items.Count >= 8)
                                {
                                    _item.InsideInventories.Remove(item.uniq);
                                    _item.Spawn(playerToItemsPair.Key, item);
                                    continue;
                                }

                                playerToItemsPair.Key.ReferenceHub.inventory.items.Add(item);
                            }
                        }
                    });
                }
            }

            public static void OnWaitingForPlayers(WaitingForPlayersEvent ev)
            {
                foreach (CustomItem _item in Registered)
                    _item.OnWaitingForPlayers();
            }

            public static void OnRoundStart()
            {
                foreach (CustomItem item in Registered)
                {
                    item?.SpawnAll();
                }
            }
        }
    }
}
