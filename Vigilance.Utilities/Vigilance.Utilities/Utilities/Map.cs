using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Vigilance.API;
using Vigilance.API.Enums;

using Vigilance.Extensions;

using Interactables.Interobjects.DoorUtils;

using Mirror;
using UnityEngine;
using GameCore;
using RemoteAdmin;
using Grenades;

using Object = UnityEngine.Object;

namespace Vigilance.Utilities
{
    public static class MapUtilities
    {
        private static List<GameObject> _dummies = new List<GameObject>();

        public static List<GameObject> Dummies => _dummies;

        public static void ClearDummies()
        {
            foreach (GameObject dummy in _dummies)
            {
                NetworkServer.Destroy(dummy);
            }

            _dummies.Clear();
        }

        public static Vector3 FindSafePosition(Vector3 pos)
        {
            return new Vector3(pos.x, pos.y + 2f, pos.z);
        }

        public static int GenerateSeed()
        {
            int seed = 0;

            int num = ConfigFile.ServerConfig.GetInt("map_seed", -1);

            if (num < 1)
                num = UnityEngine.Random.Range(1, int.MaxValue);

            seed = Mathf.Clamp(num, 1, int.MaxValue);

            Common.DoUntilFalse(() => PluginManager.Config.BlacklistedSeeds.Contains(seed.ToString()), () =>
            {
                if (num < 1)
                    num = UnityEngine.Random.Range(1, int.MaxValue);

                seed = Mathf.Clamp(num, 1, int.MaxValue);

                Log.Add($"Generated a blacklisted seed [{seed}]! Regenerating ..");
            });

            return seed;
        }

        public static GameObject SpawnDummyModel(Vector3 position, Quaternion rotation, RoleType role, float x, float y, float z)
        {
            try
            {
                GameObject obj = Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
                CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
                ccm.CurClass = role;
                ccm.RefreshModel(role);
                obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
                obj.GetComponent<QueryProcessor>().PlayerId = 9999;
                obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
                obj.transform.localScale = new Vector3(x, y, z);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                NetworkServer.Spawn(obj);
                _dummies.Add(obj);
                return obj;
            }
            catch (Exception e)
            {
                Log.Add(nameof(SpawnDummyModel), e);
                return null;
            }
        }

        public static Grenade SpawnGrenade(Player player, GrenadeType grenadeType) => SpawnGrenade(player.Position, grenadeType);

        public static Grenade SpawnGrenade(Vector3 pos, GrenadeType type)
        {
            if (!CompCache.Player.HasItem((ItemType)(int)type))
                CompCache.Player.AddItem((ItemType)(int)type);

            GrenadeManager gm = CompCache.GrenadeManager;
            Grenade grenade = Object.Instantiate(gm.availableGrenades.Where(h => (int)h.inventoryID == (int)type).FirstOrDefault().grenadeInstance.GetComponent<Grenade>());
            grenade.InitData(gm, Vector3.zero, Vector3.zero, 0f);
            grenade.transform.position = pos;
            NetworkServer.Spawn(grenade.gameObject);
            return grenade;
        }

        public static Grenade SpawnGrenade(Vector3 pos, Vector3 direction, Vector3 velocity, float force, GrenadeType type)
        {
            if (!CompCache.Player.HasItem((ItemType)(int)type))
                CompCache.Player.AddItem((ItemType)(int)type);

            GrenadeManager gm = CompCache.GrenadeManager;
            Grenade grenade = Object.Instantiate(gm.availableGrenades.Where(h => (int)h.inventoryID == (int)type).FirstOrDefault().grenadeInstance.GetComponent<Grenade>());
            grenade.InitData(gm, velocity, direction, force);
            grenade.transform.position = pos;
            NetworkServer.Spawn(grenade.gameObject);
            return grenade;
        }

        public static Room FindParentRoom(GameObject objectInRoom)
        {
            var rooms = Map.Rooms.ToList();

            Room room = null;

            const string playerTag = "Player";

            if (!objectInRoom.CompareTag(playerTag))
            {
                room = objectInRoom.GetComponentInParent<Room>();
            }
            else
            {
                var ply = PlayersList.GetPlayer(objectInRoom);

                if (ply.Role == RoleType.Scp079)
                    room = FindParentRoom(ply.ReferenceHub.scp079PlayerScript.currentCamera.gameObject);
            }

            if (room == null)
            {
                Ray ray = new Ray(objectInRoom.transform.position, Vector3.down);

                if (Physics.RaycastNonAlloc(ray, Cacher.RayCache, 10, 1 << 0, QueryTriggerInteraction.Ignore) == 1)
                    room = Cacher.RayCache[0].collider.gameObject.GetComponentInParent<Room>();
            }

            if (room == null && rooms.Count != 0)
                room = rooms[rooms.Count - 1];

            return room;
        }

        public static Camera079 GetCamera(ushort id)
        {
            foreach (Camera079 camera in Scp079PlayerScript.allCameras)
            {
                if (camera.cameraId == id)
                    return camera;
            }

            return null;
        }

        public static Camera079 GetCamera(API.Enums.CameraType type) => GetCamera((ushort)type);

        public static DoorVariant GetDoor(string doorName)
        {
            DoorNametagExtension.NamedDoors.TryGetValue(doorName, out var nameExtension);
            return nameExtension == null ? null : nameExtension.TargetDoor;
        }

        public static void ChangeUnitColor(int index, string color)
        {
            var unit = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames[index].UnitName;
            Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Remove(Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames[index]);
            Respawning.NamingRules.UnitNamingRules.AllNamingRules[Respawning.SpawnableTeamType.NineTailedFox].AddCombination($"<color={color}>{unit}</color>", Respawning.SpawnableTeamType.NineTailedFox);

            foreach (var ply in PlayersList.List.Where(x => x.ReferenceHub.characterClassManager.CurUnitName == unit))
            {
                var modifiedUnit = Regex.Replace(unit, "<[^>]*?>", string.Empty);

                if (!string.IsNullOrEmpty(color))
                    modifiedUnit = $"<color={color}>{modifiedUnit}</color>";

                ply.ReferenceHub.characterClassManager.NetworkCurUnitName = modifiedUnit;
            }
        }

        public static global::Ragdoll SpawnRagdoll(global::Role role, Ragdoll.Info ragdollInfo, Vector3 position, Quaternion rotation = default, Vector3 velocity = default, bool allowRecall = false)
        {
            if (role.model_ragdoll == null)
                return null;

            GameObject gameObject = Object.Instantiate(role.model_ragdoll, position + role.ragdoll_offset.position, Quaternion.Euler(rotation.eulerAngles + role.ragdoll_offset.rotation));
            global::Ragdoll ragdollObject = gameObject.GetComponent<global::Ragdoll>();
            ragdollObject.Networkowner = ragdollInfo != null ? ragdollInfo : MapCache.DefaultRagdollOwner;
            ragdollObject.NetworkallowRecall = allowRecall;
            ragdollObject.NetworkPlayerVelo = velocity;
            NetworkServer.Spawn(gameObject);
            return ragdollObject;
        }

        public static global::Ragdoll SpawnRagdoll(RoleType roleType, string victimNick, global::PlayerStats.HitInfo hitInfo, Vector3 position, Quaternion rotation = default, Vector3 velocity = default, bool allowRecall = false, int playerId = -1, string mirrorOwnerId = null)
        {
            global::Role role = CharacterClassManager._staticClasses.SafeGet(roleType);

            if (role.model_ragdoll == null)
                return null;

            var @default = MapCache.DefaultRagdollOwner;
            var ragdollInfo = new Ragdoll.Info()
            {
                ownerHLAPI_id = mirrorOwnerId != null ? mirrorOwnerId : @default.ownerHLAPI_id,
                PlayerId = playerId,
                DeathCause = hitInfo != default ? hitInfo : @default.DeathCause,
                ClassColor = role.classColor,
                FullName = role.fullName,
                Nick = victimNick,
            };

            return SpawnRagdoll(role, ragdollInfo, position, rotation, velocity, allowRecall);
        }

        public static global::Ragdoll SpawnRagdoll(RoleType roleType, DamageTypes.DamageType deathCause, string victimNick, Vector3 position, Quaternion rotation = default, Vector3 velocity = default, bool allowRecall = false, int playerId = -1, string mirrorOwnerId = null)
        {
            var @default = MapCache.DefaultRagdollOwner;
            return SpawnRagdoll(roleType, victimNick, new PlayerStats.HitInfo(@default.DeathCause.Amount, @default.DeathCause.Attacker, deathCause, -1), position, rotation, velocity, allowRecall, playerId, mirrorOwnerId);
        }

        public static global::Ragdoll SpawnRagdoll(Player victim, DamageTypes.DamageType deathCause, Vector3 position, Quaternion rotation = default, Vector3 velocity = default, bool allowRecall = true)
        {
            return SpawnRagdoll(victim.Role, deathCause, victim.Nick, position, rotation, velocity, allowRecall, victim.PlayerId, victim.GameObject.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId);
        }

        public static void Broadcast(API.Broadcast bc, IEnumerable<Player> users)
        {
            foreach (Player player in users)
            {
                player.Broadcast(bc);
            }
        }

        public static void Broadcast(string text, bool mono, int duration, IEnumerable<Player> players)
        {
            foreach (Player player in players)
            {
                player.Broadcast(text, duration, mono);
            }
        }

        public static Vector3 GetRandomSpawnpoint(RoleType role)
        {
            GameObject randomPosition = CharacterClassManager._spawnpointManager.GetRandomPosition(role);
            return randomPosition == null ? Vector3.zero : randomPosition.transform.position;
        }

        public static Room GetRoom(RoomType roomType)
        {

            foreach (Room room in Map.Rooms)
            {
                if (room.Type == roomType)
                    return room;
            }

            return null;
        }

        public static RoomInformation GetRoomInformation(string search)
        {
            foreach (RoomInformation info in FindObjects<RoomInformation>())
            {
                if (info.name == search || info.name.Contains(search) || info.tag == search || info.tag.Contains(search))
                    return info;
            }

            foreach (RoomInformation.RoomType type in Common.GetEnums<RoomInformation.RoomType>())
            {
                string str = type.ToString();
                if (str == search || str.ToLower() == search.ToLower() || str.Contains(search))
                    return GetRoomInformation(type);
            }

            return null;
        }

        public static RoomInformation GetRoomInformation(RoomInformation.RoomType type)
        {
            foreach (RoomInformation info in FindObjects<RoomInformation>())
            {
                if (info.CurrentRoomType == type)
                    return info;
            }

            return null;
        }

        public static RoomInformation GetRoomInformation(RoomType type)
        {
            Room room = GetRoom(type);

            if (room != null)
                return room.GetComponent<RoomInformation>();

            return null;
        }

        public static Room GetRoom(string name)
        {
            try
            {
                foreach (RoomType type in Common.GetEnums<RoomType>())
                {
                    if (type.ToString().ToLower() == name.ToLower())
                        return GetRoom(type);
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Add(nameof(GetRoom), e);
                return null;
            }
        }

        public static T Find<T>() where T : Component
        {
            return Object.FindObjectOfType<T>();
        }

        public static IEnumerable<T> FindObjects<T>() where T : Component
        {
            return Object.FindObjectsOfType<T>();
        }

        public static void OnRoundRestart()
        {
            try
            {
                MapCache.Clear();
            }
            catch (Exception e)
            {
                Log.Add("MapUtilities", $"An error occured while resetting MapCache!\n{e}", LogType.Error);
            }
        }

        public static void OnMapGenerated()
        {
            try
            {
                MapCache.Refresh();
            }
            catch (Exception e)
            {
                Log.Add("MapUtilities", $"An error occured while filling the MapCache!\n{e}", LogType.Error);
            }
        }

        public static void OnRoundStart() => CompCache.Refresh();

        public static Pickup SpawnItem(ItemType item, Vector3 pos, Quaternion rot) => CompCache.Inventory.SetPickup(item, Parser.NegativeInfinity, pos, rot, 0, 0, 0, true);
        public static Pickup SpawnItem(Inventory.SyncItemInfo itemInfo, Vector3 pos, Quaternion rot) => CompCache.Inventory.SetPickup(itemInfo.id, itemInfo.durability, pos, rot, itemInfo.modSight, itemInfo.modBarrel, itemInfo.modOther, true);
    }
}
