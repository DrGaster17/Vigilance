using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using Vigilance.API.Enums;
using UnityEngine;
using Mirror;
using Vigilance.Utilities;
using Vigilance.Extensions;
using Vigilance.Patching.Patches;
using MapGeneration;
using System;

namespace Vigilance.API
{
    public class Door
    {
        private DoorVariant _door;

        public Door(DoorVariant d, Room room, DoorType type, int id, string tag)
        {
            _door = d;
            Name = tag;
            Id = id;
            Room = room;
            Type = type;
            IsLocked = false;
            DisallowedPlayers = new List<Player>();
        }

        public bool AllowBypassOverride { get; set; }
        public bool IsLocked { get; set; }
        public int Id { get; }
        public string Name { get; }
        public Room Room { get; }
        public DoorType Type { get; }
        public List<Player> DisallowedPlayers { get; }
        public DoorVariant Prefab => _door.TryGetComponent(out DoorSpawnpoint p) ? p.TargetPrefab : null;

        public Vector3 Position { get => _door.transform.position; set => SetPosition(value); }
        public Vector3 Scale { get => _door.transform.localScale; set => SetScale(value); }
        public Quaternion Rotation { get => _door.transform.rotation; set => SetRotation(value); }
        public bool IsOpen { get => _door.NetworkTargetState; set => _door.NetworkTargetState = value; }
        public bool IsDestroyed { get => GetIsDestroyed(); set => SetDestroyedState(value); }
        public bool IsDestroyable => _door is IDamageableDoor damage && damage != null;
        public bool IsCheckpoint => _door is CheckpointDoor chck && chck != null;

        public GameObject GameObject => _door.gameObject;
        public KeycardPermissions Permissions => _door.RequiredPermissions.RequiredPermissions;

        public bool ScpOverride => _door.RequiredPermissions.RequiredPermissions.HasFlagFast(KeycardPermissions.ScpOverride);

        public void ChangeLock(DoorLockReason reason, bool state) => _door.ServerChangeLock(reason, state);

        public void Destroy()
        {
            if (IsDestroyable)
            {
                (_door as IDamageableDoor).IsDestroyed = true;
            }
        }

        public bool CanUse(Player player, out bool cooldown)
        {
            if (player.PlayerLock || (DisallowedPlayers.Contains(player) || IsLocked) && !AllowBypassOverride)
            {
                cooldown = false;
                return false;
            }

            if (player.BypassMode || player.Role == RoleType.Scp079)
            {
                cooldown = false;
                return true;
            }

            if (!_door.AllowInteracting(player.ReferenceHub, 0))
            {
                cooldown = true;
                return false;
            }

            if (player.IsSCP && (_door is CheckpointDoor chck && chck != null))
            {
                cooldown = false;
                return true;
            }

            if (_door.RequiredPermissions.RequiredPermissions == KeycardPermissions.None)
            {
                cooldown = false;
                return true;
            }

            if (PluginManager.Config.RemoteCard)
            {
                foreach (Inventory.SyncItemInfo itemInfo in player.Items)
                {
                    if (!itemInfo.id.IsKeycard())
                        continue;

                    string[] permissions = player.ReferenceHub.inventory.GetItemByID(itemInfo.id).permissions;
                    KeycardPermissions translatedPermissions = DoorPermissionUtils.TranslateObsoletePermissions(permissions);

                    if (!_door.RequiredPermissions.RequireAll)
                    {
                        if ((translatedPermissions & _door.RequiredPermissions.RequiredPermissions) > KeycardPermissions.None)
                        {
                            cooldown = false;
                            return true;
                        }
                    }
                    else
                    {
                        if ((translatedPermissions & _door.RequiredPermissions.RequiredPermissions) == _door.RequiredPermissions.RequiredPermissions)
                        {
                            cooldown = false;
                            return true;
                        }
                    }
                }
            }
            else
            {
                Item inHand = player.ReferenceHub.inventory.GetItemByID(player.ReferenceHub.inventory.curItem);

                if (inHand != null && inHand.id.IsKeycard())
                {
                    KeycardPermissions translatedPermissions = DoorPermissionUtils.TranslateObsoletePermissions(inHand.permissions);

                    if (!_door.RequiredPermissions.RequireAll)
                    {
                        if ((translatedPermissions & _door.RequiredPermissions.RequiredPermissions) > KeycardPermissions.None)
                        {
                            cooldown = false;
                            return true;
                        }
                    }
                    else
                    {
                        if ((translatedPermissions & _door.RequiredPermissions.RequiredPermissions) == _door.RequiredPermissions.RequiredPermissions)
                        {
                            cooldown = false;
                            return true;
                        }
                    }
                }
                else
                {
                    cooldown = false;
                    return false;
                }
            }

            cooldown = false;
            return false;
        }

        public void Delete()
        {
            NetworkServer.Destroy(GameObject);
        }

        public void Lock()
        {
            IsLocked = true;
            _door.ServerChangeLock(DoorLockReason.AdminCommand, true);
        }

        public void Unlock()
        {
            IsLocked = false;
            _door.ServerChangeLock(DoorLockReason.AdminCommand, false);
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void ChangeState()
        {
            _door.NetworkTargetState = !_door.TargetState;
        }

        public Door Clone() => CloneAt(Position, Rotation, Scale);

        public Door CloneAt(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            try
            {
                DoorVariant prefab = null;

                if (DoorExtensions.EntranceZoneDoors.Contains(Type))
                    prefab = GetPrefabFromZone(DoorZone.Ez);
                else if (DoorExtensions.HeavyContainmentDoors.Contains(Type))
                    prefab = GetPrefabFromZone(DoorZone.Hcz);
                else
                    prefab = GetPrefabFromZone(DoorZone.Lcz);

                if (prefab != null)
                {
                    Log.Debug("Door", $"Prefab for door {Name} - {Type} has been found.");

                    DoorVariant door = UnityEngine.Object.Instantiate(prefab);

                    door.gameObject.AddComponent<DoorNametagExtension>().UpdateName(Name);

                    door.transform.position = pos;
                    door.transform.rotation = rot;
                    door.transform.localScale = scale;

                    NetworkServer.Spawn(door.gameObject);

                    Door api = new Door(door, Room, Type, door.GetInstanceID(), Name);

                    DoorExtensions.Doors.Add(api.Id, api);

                    return api;
                }
                else
                {
                    Log.Warn("Door", $"Tried to clone a door ({Name} - {Type}), but the prefab is null.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Add("Door", e);
                return null;
            }
        }

        private void SetDestroyedState(bool state)
        {
            if (IsDestroyed)
            {
                (_door as IDamageableDoor).IsDestroyed = state;
            }
        }

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

        private bool GetIsDestroyed()
        {
            if (_door is IDamageableDoor damage)
            {
                return damage.IsDestroyed;
            }
            else
            {
                return false;
            }
        }

        public static DoorVariant SpawnVariant(DoorZone type, Vector3 pos, Vector3 scale, Quaternion rot, string name = "")
        {
            DoorVariant prefab = GetPrefabFromZone(type);

            if (prefab == null)
                return null;

            DoorVariant variant = UnityEngine.Object.Instantiate(prefab, pos, rot);

            variant.transform.localScale = scale;

            if (!string.IsNullOrEmpty(name))
                variant.gameObject.AddComponent<DoorNametagExtension>().UpdateName(name);

            NetworkServer.Spawn(variant.gameObject);        

            return variant;
        }

        public static Door Spawn(DoorZone zone, Vector3 pos, Vector3 scale, Quaternion rot, string name = "")
        {
            DoorVariant variant = SpawnVariant(zone, pos, scale, rot, name);

            if (variant == null)
                return null;

            Door door = new Door(variant, MapUtilities.FindParentRoom(variant.gameObject), DoorExtensions.GetDoorType(variant.TryGetComponent(out DoorNametagExtension tag) ? tag.GetName : variant.name), variant.GetInstanceID(), name);

            if (!DoorExtensions.Doors.ContainsKey(variant.GetInstanceID()))
                DoorExtensions.Doors.Add(variant.GetInstanceID(), door);

            return door;
        }

        private static DoorVariant GetPrefabFromZone(DoorZone zone)
        {
            if (zone == DoorZone.Ez)
                return DoorSpawnpoint_SetupAllDoors.EzPrefab;

            if (zone == DoorZone.Hcz)
                return DoorSpawnpoint_SetupAllDoors.HczPrefab;

            if (zone == DoorZone.Lcz)
                return DoorSpawnpoint_SetupAllDoors.LczPrefab;

            return null;
        }

        public static Door CheckpointA { get; set; }
        public static Door CheckpointB { get; set; }
        public static Door CheckpointEZ { get; set; }

        public static Door Scp173Gate { get; set; }

        public static DoorVariant EzPrefab => GetPrefabFromZone(DoorZone.Ez);
        public static DoorVariant HczPrefab => GetPrefabFromZone(DoorZone.Hcz);
        public static DoorVariant LczPrefab => GetPrefabFromZone(DoorZone.Lcz);
    }
}