using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Mirror;
using UnityEngine;
using Grenades;
using Vigilance.API.Enums;
using Vigilance.API;
using Object = UnityEngine.Object;
using RemoteAdmin;
using Scp914;
using GameCore;
using System.IO;
using Vigilance.External.Extensions;
using Vigilance.External.Patching;
using Interactables.Interobjects.DoorUtils;
using System.Text.RegularExpressions;
using CustomPlayerEffects;
using MapGeneration;
using System.Globalization;
using System.Diagnostics;

namespace Vigilance.External.Utilities
{
    public static class PlayerUtilities
    {
        public static List<Ragdoll> GetRagdolls(Player p)
        {
            if (!PatchData.Ragdolls.ContainsKey(p))
                PatchData.Ragdolls.Add(p, new List<Ragdoll>());

            return PatchData.Ragdolls[p];
        }

        public static List<Pickup> GetPickups(Player p)
        {
            if (!PatchData.Pickups.ContainsKey(p))
                PatchData.Pickups.Add(p, new List<Pickup>());

            return PatchData.Pickups[p];
        }

        public static string GetGroupNode(UserGroup group)
        {
            if (group == null) 
                return "";

            PermissionsHandler ph = ServerStatic.PermissionsHandler;
            if (ph == null) 
                throw new Exception("Update your Remote Admin config!");

            foreach (KeyValuePair<string, UserGroup> pair in ph.GetAllGroups())
            {
                if (pair.Value == group || (pair.Value.BadgeColor == group.BadgeColor && pair.Value.BadgeText == group.BadgeText && pair.Value.Cover == group.Cover && pair.Value.HiddenByDefault == group.HiddenByDefault && pair.Value.KickPower == group.KickPower && pair.Value.Permissions == group.Permissions && pair.Value.RequiredKickPower == group.RequiredKickPower && pair.Value.Shared == group.Shared))
                {
                    return pair.Key;
                }
            }

            return "";
        }

        public static bool GetBadgeHidden(ServerRoles roles) => string.IsNullOrEmpty(roles.HiddenBadge);
        public static void SetBadgeHidden(bool value, Player player)
        {
            if (value) player.ReferenceHub.characterClassManager.CmdRequestHideTag(); else player.ReferenceHub.characterClassManager.CallCmdRequestShowTag(false);
        }

        public static ulong GetParsedId(Player player)
        {
            string id = player.IsHost ? "Dedicated Server" : player.UserId.Split('@')[0];

            if (id == "Dedicated Server") 
                return 0;

            if (!ulong.TryParse(id, out ulong user)) 
                return 0;

            return user;
        }

        public static UserIdType GetIdType(Player player)
        {
            if (string.IsNullOrEmpty(player.UserId)) 
                return UserIdType.Server;

            if (player.UserId.EndsWith("@steam")) 
                return UserIdType.Steam;

            if (player.UserId.EndsWith("@discord")) 
                return UserIdType.Discord;

            if (player.UserId.EndsWith("@northwood")) 
                return UserIdType.Northwood;

            if (player.UserId.EndsWith("@patreon")) 
                return UserIdType.Patreon;

            return UserIdType.Unspecified;
        }

        public static Room GetRoom(Player player) => MapUtilities.FindParentRoom(player.GameObject);

        public static void SetScale(Player player, Vector3 scale)
        {
            try
            {
                NetworkIdentity identity = player.ConnectionIdentity;
                player.GameObject.transform.localScale = scale;
                ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
                destroyMessage.netId = identity.netId;

                foreach (GameObject obj in PlayerManager.players)
                {
                    NetworkConnection playerCon = player.Connection;

                    if (obj != player.GameObject) 
                        playerCon.Send(destroyMessage, 0);

                    NetworkServer.SendSpawnMessage(identity, playerCon);
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        public static void AddItem(Player player, ItemType item)
        {
            if (player.ReferenceHub.inventory.items.Count >= 8) 
                MapUtilities.SpawnItem(item, player.Position, player.Rotations); else player.ReferenceHub.inventory.AddNewItem(item);
        }

        public static void SetExperience(Player player, float exp)
        {
            if (player.ReferenceHub.scp079PlayerScript == null)
                return;

            player.ReferenceHub.scp079PlayerScript.Exp = exp;
            player.ReferenceHub.scp079PlayerScript.OnExpChange();
        }

        public static void SetLevel(Player player, byte level)
        {
            if (player.ReferenceHub.scp079PlayerScript == null || player.ReferenceHub.scp079PlayerScript.Lvl == level)
                return;

            player.ReferenceHub.scp079PlayerScript.Lvl = level;
            player.ReferenceHub.scp079PlayerScript.TargetLevelChanged(player.Connection, level);
        }

        public static void SetMaxEnergy(Player player, float value)
        {
            if (player.ReferenceHub.scp079PlayerScript == null)
                return;

            player.ReferenceHub.scp079PlayerScript.NetworkmaxMana = value;
            player.ReferenceHub.scp079PlayerScript.levels[player.ReferenceHub.scp079PlayerScript.Lvl].maxMana = value;
        }

        public static void SetEnergy(Player player, float value)
        {
            if (player.ReferenceHub.scp079PlayerScript == null)
                return;

            player.ReferenceHub.scp079PlayerScript.Mana = value;
        }

        public static bool GetActiveEffect<T>(Player player) where T : PlayerEffect
        {
            if (player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(T), out PlayerEffect playerEffect))
                return playerEffect.Enabled;

            return false;
        }

        public static void DisableAllEffects(Player player)
        {
            foreach (KeyValuePair<Type, PlayerEffect> effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.Value.Enabled)
                    effect.Value.ServerDisable();
            }
        }

        public static PlayerEffect GetEffect(Player player, EffectType effect)
        {
            bool found = player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(effect.Type(), out var playerEffect);
            return found ? null : playerEffect;
        }

        public static bool TryGetEffect(Player player, EffectType effect, out PlayerEffect playerEffect)
        {
            playerEffect = GetEffect(player, effect);
            return playerEffect != null;
        }

        public static byte GetEffectIntensity<T>(Player player) where T : PlayerEffect
        {
            if (player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(T), out PlayerEffect playerEffect))
                return playerEffect.Intensity;

            throw new ArgumentException("The given type is invalid.");
        }

        public static void EnableEffect(Player player, EffectType effect, float duration = 0f, bool addDurationIfActive = false)
        {
            if (TryGetEffect(player, effect, out var pEffect))
                player.ReferenceHub.playerEffectsController.EnableEffect(pEffect, duration, addDurationIfActive);
        }

        public static void DisableEffect(Player player, EffectType effect)
        {
            if (TryGetEffect(player, effect, out var playerEffect))
                playerEffect.ServerDisable();
        }

        public static void DisableEffect<T>(Player player) where T : PlayerEffect => player.ReferenceHub.playerEffectsController.DisableEffect<T>();
        public static void EnableEffect<T>(Player player, float duration = 0f, bool addDurationIfActive = false) where T : PlayerEffect => player.ReferenceHub.playerEffectsController.EnableEffect<T>(duration, addDurationIfActive);
        public static void EnableEffect(Player player, PlayerEffect effect, float duration = 0f, bool addDurationIfActive = false) => player.ReferenceHub.playerEffectsController.EnableEffect(effect, duration, addDurationIfActive);
        public static bool EnableEffect(Player player, string effect, float duration = 0f, bool addDurationIfActive = false) => player.ReferenceHub.playerEffectsController.EnableByString(effect, duration, addDurationIfActive);
        public static void ChangeEffectIntensity<T>(Player player, byte intensity) where T : PlayerEffect => player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<T>(intensity);
        public static void ChangeEffectIntensity(Player player, string effect, byte intensity, float duration = 0) => player.ReferenceHub.playerEffectsController.ChangeByString(effect, intensity, duration);

        public static void Rocket(Player player, float speed) => Timing.RunCoroutine(Coroutines.Rocket(player, speed));
    }

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

            CommonUtilities.DoUntilFalse(() => PluginManager.Config.BlacklistedSeeds.Contains(seed.ToString()), () =>
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
            if (!LocalComponents.Player.HasItem((ItemType)(int)type)) 
                LocalComponents.Player.AddItem((ItemType)(int)type);

            GrenadeManager gm = LocalComponents.GrenadeManager;
            Grenade grenade = Object.Instantiate(gm.availableGrenades.Where(h => (int)h.inventoryID == (int)type).FirstOrDefault().grenadeInstance.GetComponent<Grenade>());
            grenade.InitData(gm, Vector3.zero, Vector3.zero, 0f);
            grenade.transform.position = pos;
            NetworkServer.Spawn(grenade.gameObject);
            return grenade;
        }

        public static Grenade SpawnGrenade(Vector3 pos, Vector3 direction, Vector3 velocity, float force, GrenadeType type)
        {
            if (!LocalComponents.Player.HasItem((ItemType)(int)type)) 
                LocalComponents.Player.AddItem((ItemType)(int)type);

            GrenadeManager gm = LocalComponents.GrenadeManager;
            Grenade grenade = Object.Instantiate(gm.availableGrenades.Where(h => (int)h.inventoryID == (int)type).FirstOrDefault().grenadeInstance.GetComponent<Grenade>());
            grenade.InitData(gm, velocity, direction, force);
            grenade.transform.position = pos;
            NetworkServer.Spawn(grenade.gameObject);
            return grenade;
        }

        public static Room FindParentRoom(GameObject objectInRoom)
        {
            var rooms = Map.Rooms;

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

                if (Physics.RaycastNonAlloc(ray, LocalCache.RayCache, 10, 1 << 0, QueryTriggerInteraction.Ignore) == 1)
                    room = LocalCache.RayCache[0].collider.gameObject.GetComponentInParent<Room>();
            }

            if (room == null && rooms.Count() != 0)
                room = rooms.ToList()[rooms.Count() - 1];

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
            ragdollObject.Networkowner = ragdollInfo != null ? ragdollInfo : LocalCache.DefaultRagdollOwner;
            ragdollObject.NetworkallowRecall = allowRecall;
            ragdollObject.NetworkPlayerVelo = velocity;
            NetworkServer.Spawn(gameObject);
            return ragdollObject;
        }

        public static global::Ragdoll SpawnRagdoll(RoleType roleType, string victimNick, global::PlayerStats.HitInfo hitInfo, Vector3 position,  Quaternion rotation = default, Vector3 velocity = default, bool allowRecall = false, int playerId = -1, string mirrorOwnerId = null)
        {
            global::Role role = CharacterClassManager._staticClasses.SafeGet(roleType);

            if (role.model_ragdoll == null)
                return null;

            var @default = LocalCache.DefaultRagdollOwner;
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
            var @default = LocalCache.DefaultRagdollOwner;
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

            foreach (RoomInformation.RoomType type in EnumUtilities.GetValues<RoomInformation.RoomType>())
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
                foreach (RoomType type in EnumUtilities.GetValues<RoomType>())
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

        public static void OnRoundStart()
        {
            LocalComponents.Refresh();
        }

        public static Pickup SpawnItem(ItemType item, Vector3 pos, Quaternion rot) => LocalComponents.Inventory.SetPickup(item, LocalCache.Infinity, pos, rot, 0, 0, 0, true);
        public static Pickup SpawnItem(Inventory.SyncItemInfo itemInfo, Vector3 pos, Quaternion rot) => LocalComponents.Inventory.SetPickup(itemInfo.id, itemInfo.durability, pos, rot, itemInfo.modSight, itemInfo.modBarrel, itemInfo.modOther, true);
    }

    public static class MapCache
    {
        private static string _outsitePanelName = "OutsitePanelScript";
        private static string _femurName = "FemurBreaker";
        private static string _intercomName = "IntercomSpeakingZone";
        private static string _pocketName = "HeavyRooms/PocketWorld";
        private static string _roomTag = "Room";
        private static string _pdExitTag = "PD_EXIT";
        private static string _surfaceTag = "Outside";

        private static AlphaWarheadController _awc;
        private static AlphaWarheadNukesitePanel _nukesite;
        private static AlphaWarheadOutsitePanel _outsite;
        private static OneOhSixContainer _ooc;
        private static LureSubjectContainer _cont;
        private static GameObject _outsitePanelScript;
        private static GameObject _femurBreaker;
        private static GameObject _intercomZone;
        private static GameObject _pocket;
        private static GameObject _surface;

        private static List<GameObject> _roomsObjects = new List<GameObject>();
        private static List<FlickerableLight> _lights = new List<FlickerableLight>();
        private static List<FlickerableLightController> _controllers = new List<FlickerableLightController>();
        private static List<GameObject> _pocketExits = new List<GameObject>();
        private static List<Room> _rooms = new List<Room>();

        public static GameObject OutsitePanelScript => _outsitePanelScript == null ? (_outsitePanelScript = GameObject.Find(_outsitePanelName)) : _outsitePanelScript;
        public static GameObject FemurBreaker => _femurBreaker == null ? (_femurBreaker = GameObject.Find(_femurName)) : _femurBreaker;
        public static GameObject IntercomZone => _intercomZone == null ? (_intercomZone = GameObject.Find(_intercomName)) : _intercomZone;
        public static GameObject PocketDimension => _pocket == null ? (_pocket = GameObject.Find(_pocketName)) : _pocket;
        public static GameObject Surface => _surface == null ? (_surface = GameObject.Find(_surfaceTag)) : _surface;
        public static AlphaWarheadController AlphaWarhead => _awc == null ? (_awc = AlphaWarheadController.Host) : _awc;
        public static AlphaWarheadNukesitePanel Nukesite => _nukesite == null ? (_nukesite = AlphaWarheadOutsitePanel.nukeside) : _nukesite;
        public static AlphaWarheadOutsitePanel Outsite => _outsite == null ? (_outsite = OutsitePanelScript.GetComponent<AlphaWarheadOutsitePanel>()) : _outsite;
        public static OneOhSixContainer OneOhSixContainer => _ooc == null ? (_ooc = MapUtilities.Find<OneOhSixContainer>()) : _ooc;
        public static LureSubjectContainer LureSubjectContainer => _cont == null ? (_cont = MapUtilities.Find<LureSubjectContainer>()) : _cont;

        public static IEnumerable<GameObject> RoomObjects => _roomsObjects;
        public static IEnumerable<FlickerableLight> Lights => _lights;
        public static IEnumerable<FlickerableLightController> Controllers => _controllers;
        public static IEnumerable<GameObject> PdExits => _pocketExits;
        public static IEnumerable<Room> Rooms => _rooms;

        public static void Refresh()
        {
            try
            {
                _outsitePanelScript = GameObject.Find(_outsitePanelName);
                _femurBreaker = GameObject.FindGameObjectWithTag(_femurName);
                _intercomZone = GameObject.Find(_intercomName);

                _pocket = GameObject.Find(_pocketName);

                _outsite = _outsitePanelScript.GetComponent<AlphaWarheadOutsitePanel>();

                _awc = AlphaWarheadController.Host;
                _nukesite = AlphaWarheadOutsitePanel.nukeside;

                _ooc = MapUtilities.Find<OneOhSixContainer>();
                _cont = MapUtilities.Find<LureSubjectContainer>();

                _roomsObjects.AddRange(GameObject.FindGameObjectsWithTag(_roomTag));
                _roomsObjects.Add(PocketDimension);
                _roomsObjects.Add(Surface);

                _lights.AddRange(MapUtilities.FindObjects<FlickerableLight>());
                _controllers.AddRange(MapUtilities.FindObjects<FlickerableLightController>());
                _pocketExits.AddRange(GameObject.FindGameObjectsWithTag(_pdExitTag));

                _roomsObjects.ForEach((x) => _rooms.Add(x.AddComponent<Room>()));

                DoorExtensions.SetInfo();
                WorkstationExtensions.SetInfo();
                CameraExtensions.SetInfo();
                LiftExtensions.SetInfo();
                TeslaExtensions.SetInfo();
                GeneratorExtensions.SetInfo();
                LockerExtensions.SetInfo();
                WindowExtensions.SetInfo();
            }
            catch (Exception e)
            {
                Log.Add("MapCache", $"An error occured while refreshing cache!\n{e}", LogType.Error);
            }
        }

        public static void RefreshPdExits()
        {
            _pocketExits.Clear();
            _pocketExits.AddRange(GameObject.FindGameObjectsWithTag(_pdExitTag));
        }

        public static void Clear()
        {
            try
            {
                _roomsObjects.Clear();
                _lights.Clear();
                _controllers.Clear();
                _pocketExits.Clear();
                _rooms.Clear();

                DoorExtensions.Doors.Clear();
                DoorExtensions.Types.Clear();
                WorkstationExtensions.Workstations.Clear();
                CameraExtensions.Types.Clear();
                CameraExtensions.Cameras.Clear();
                LiftExtensions.Elevators.Clear();
                LiftExtensions.OrderedElevatorTypes.Clear();
                TeslaExtensions.Gates.Clear();
                GeneratorExtensions.Generators.Clear();
                LockerExtensions.Lockers.Clear();
                WindowExtensions.Windows.Clear();

                MapUtilities.ClearDummies();
            }
            catch (Exception e)
            {
                Log.Add("MapCache", $"An error occured while clearing the cache!\n{e}", LogType.Error);
            }
        }
    }

    public static class LocalComponents
    {
        private static string _gameManagerName = "GameManager";

        private static GameObject _gameManager;

        private static Player _ply;
        private static PlayerStats _stats;
        private static CharacterClassManager _ccm;
        private static BanPlayer _ban;
        private static Broadcast _broadcast;
        private static GrenadeManager _gMan;
        private static Inventory _inv;
        private static SeedSynchronizer _seed;

        public static GameObject GameManager => _gameManager == null ? (_gameManager = GameObject.Find(_gameManagerName)) : _gameManager;

        public static Player Player => _ply == null ? (_ply = new Player(ReferenceHub.HostHub)) : _ply;
        public static ReferenceHub ReferenceHub => ReferenceHub.HostHub;
        public static PlayerStats PlayerStats => _stats == null ? (_stats = ReferenceHub.GetComponent<PlayerStats>()) : _stats;
        public static CharacterClassManager CharacterClassManager => _ccm == null ? (_ccm = ReferenceHub.GetComponent<CharacterClassManager>()) : _ccm;
        public static BanPlayer BanPlayer => _ban == null ? (_ban = ReferenceHub.GetComponent<BanPlayer>()) : _ban;
        public static Broadcast Broadcast => _broadcast == null ? (_broadcast = ReferenceHub.GetComponent<Broadcast>()) : _broadcast;
        public static GrenadeManager GrenadeManager => _gMan == null ? (_gMan = ReferenceHub.GetComponent<GrenadeManager>()) : _gMan;
        public static Inventory Inventory => _inv == null ? (_inv = ReferenceHub.GetComponent<Inventory>()) : _inv;
        public static SeedSynchronizer SeedSync => _seed == null ? (_seed = GameManager.GetComponent<SeedSynchronizer>()) : _seed;


        public static void Refresh()
        {
            try
            {
                _gameManager = GameObject.Find(_gameManagerName);

                GameObject host = ReferenceHub.LocalHub.gameObject;

                _ply = new Player(ReferenceHub.HostHub);
                _stats = host.GetComponent<PlayerStats>();
                _ccm = host.GetComponent<CharacterClassManager>();
                _ban = host.GetComponent<BanPlayer>();
                _broadcast = host.GetComponent<Broadcast>();
                _gMan = host.GetComponent<GrenadeManager>();
                _inv = host.GetComponent<Inventory>();
                _seed = host.GetComponent<SeedSynchronizer>();

                GC.Collect();
            }
            catch (Exception e)
            {
                Log.Add("LocalComponents", $"An error occured while refreshing components!\n{e}", LogType.Error);
            }
        }
    }

    public static class LocalCache
    {
        private static RaycastHit[] _rayCache;
        private static System.Random _random;
        public static float Infinity => -4.6566467E+11f;

        public static RaycastHit[] RayCache => _rayCache == null ? _rayCache = new RaycastHit[1] : _rayCache;
        public static System.Random RandomGen => _random == null ? _random = new System.Random() : _random;

        public static Ragdoll.Info DefaultRagdollOwner { get; } = new Ragdoll.Info()
        {
            ownerHLAPI_id = null,
            PlayerId = -1,
            DeathCause = new PlayerStats.HitInfo(-1f, "[REDACTED]", DamageTypes.Com15, -1),
            ClassColor = new Color(1f, 0.556f, 0f),
            FullName = "Class-D",
            Nick = "[REDACTED]",
        };
    }

    public static class Coroutines
    {
        public static IEnumerator<float> Rocket(Player player, float speed)
        {
            int amnt = 0;
            while (player.Role != RoleType.Spectator)
            {
                player.Position = player.Position + Vector3.up * speed;
                amnt++;
                if (amnt >= 1000)
                {
                    player.GodMode = false;
                    player.Kill();
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        public static IEnumerator<float> SpawnBodies(RagdollManager rm, int role, int count)
        {
            for (int i = 0; i < count; i++)
            {
                rm.SpawnRagdoll(rm.transform.position + Vector3.up * 5, Quaternion.identity, Vector3.zero, role, new PlayerStats.HitInfo(1000f, "WORLD", DamageTypes.Falldown, 0), false, "SCP-343", "SCP-343", 0);
                yield return Timing.WaitForSeconds(0.15f);
            }
        }
    }

    public static class CoroutineUtilities
    {
        private static List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();

        public static void StopAllCoroutines()
        {
            foreach (CoroutineHandle handle in _coroutines)
                Timing.KillCoroutines(handle);
        }

        public static CoroutineHandle StartCoroutine(IEnumerator<float> handler, string name = "")
        {
            if (handler == null) 
                return default;

            try
            {
                CoroutineHandle handle = Timing.RunCoroutine(handler, name);
                _coroutines.Add(handle);
                return handle;
            }
            catch (Exception e)
            {
                Log.Add("CoroutineUtilities", e);
                return default;
            }
        }

        public static void KillCoroutine(CoroutineHandle handle)
        {
            Timing.KillCoroutines(handle);
        }

        public static void KillCoroutines(IEnumerable<CoroutineHandle> handles)
        {
            foreach (CoroutineHandle handle in handles)
                KillCoroutine(handle);
        }

        public static void StopCoroutine(string name)
        {
            foreach (CoroutineHandle handle in _coroutines)
            {
                if (handle.Tag.ToLower() == name.ToLower() || handle.Tag.ToLower().Contains(name.ToLower()))
                {
                    KillCoroutine(handle);
                    _coroutines.Remove(handle);
                }
            }
        }
    }

    public static class Chances
    {
        public static float GetRandomNumber() => LocalCache.RandomGen.Next(1, 10000) / 100f;
        public static float GetRandomNumber(int max) => LocalCache.RandomGen.Next(1, max);
        public static bool GetRandomBool() => GetRandomNumber() < 100;

        public static bool GetChance(float chance) => chance >= (LocalCache.RandomGen.Next(1, 10000) / 100f);
    }

    public static class EnumUtilities
    {
        public static IEnumerable<T> GetValues<T>()
        {
            if (typeof(T).BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }

            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public static class CommonUtilities
    {
        /// <summary>
        /// Measures the amount of miliseconds needed for a method to complete.
        /// </summary>
        /// <param name="method">The <see cref="Action"/> (method) to measure.</param>
        /// <returns>The amount of miliseconds elapsed.</returns>
        public static long MeasureMethod(Action method)
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();

            method();

            watch.Stop();

            return watch.ElapsedMilliseconds;
        }

        public static void DoUntilTrue(Func<bool> func, Action act)
        {
            for (int i = 0; i > 0;)
            {
                if (!func())
                    act();
                else
                    return;
            }
        }

        public static void DoUntilFalse(Func<bool> func, Action act)
        {
            for (int i = 0; i > 0;)
            {
                if (func())
                    act();
                else
                    return;
            }
        }

        public static void Repeat(int times, Action act)
        {
            for (int i = 0; i < times; i++)
            {
                act();
            }
        }

        public static void RepeatDelay(int times, float delay, Action act)
        {
            for (int i = 0; i < times; i++)
            {
                Timing.CallDelayed(delay, act);
            }
        }
    }

    public static class Scp914Utilities
    {
        public static ItemType UpgradeItemId(ItemType input)
        {
            ItemType[] array = UpgradeItemIds(input);

            if (array == null) 
                return input;

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static ItemType[] UpgradeItemIds(ItemType input)
        {
            Dictionary<Scp914Knob, ItemType[]> dictionary;
            ItemType[] result;

            if (!API.Scp914.Recipes.TryGetValue(input, out dictionary) || !dictionary.TryGetValue(API.Scp914.KnobStatus, out result)) 
                return null;

            return result;
        }
    }

    public static class Parser
    {
        public const float NegativeInfinity = -4.6566467E+11f;

        public static bool ParseFloat(string input, out float result) => float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseInt(string input, out int result) => int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseLong(string input, out long result) => long.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseByte(string input, out byte result) => byte.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseSByte(string input, out sbyte result) => sbyte.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseDouble(string input, out double result) => double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseDecimal(string input, out decimal result) => decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseShort(string input, out short result) => short.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseUShort(string input, out ushort result) => ushort.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseULong(string input, out ulong result) => ulong.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseUInt(string input, out uint result) => uint.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool ParseBoolean(string input, out bool result) => bool.TryParse(input, out result);
        public static bool ParseEnum<TEnum>(string input, out TEnum result) where TEnum : struct => Enum.TryParse(input, out result);
        public static bool ParseEnum<TEnum>(string input, bool ignoreCase, out TEnum result) where TEnum : struct => Enum.TryParse(input, ignoreCase, out result);
        public static bool ParsePlayerArray(string input, out Player[] result) => ParsePlayerArray(input, '.', out result);

        /// <summary>
        /// Converts the string representation of a player array to its equivalent.
        /// </summary>
        /// <param name="input">A <see cref="string"/> representating the array to convert.</param>
        /// <param name="separator">The <see cref="char"/> to split the array by.</param>
        /// <param name="result">The converted <see cref="Player"/> <see cref="Array"/></param>
        /// <returns>true if input was converted succesfully, otherwise false.</returns>
        public static bool ParsePlayerArray(string input, char separator, out Player[] result)
        {
            if (string.IsNullOrEmpty(input))
            {
                result = new Player[] { };
                return false;
            }

            if (!input.Contains(separator))
            {
                result = new Player[] { };
                return false;
            }

            List<Player> players = new List<Player>();

            string[] args = input.Split(separator);

            foreach (string arg in args)
            {
                Player player = arg.GetPlayer();

                if (player != null)
                    players.Add(player);
            }

            result = players.ToArray();
            return true;
        }
    }

    public static class ConfigUtilities
    {
        private static string[] _lines;

        public static void AddConfig(string path, string description, string key, string value)
        {
            try
            {
                Directories.CheckFile(path);
                _lines = File.ReadAllLines(path);

                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    if (!Directories.ContainsKey(_lines, key))
                    {
                        if (!string.IsNullOrEmpty(description) && !_lines.Contains($"# {description}")) 
                            writer.WriteLine($"# {description}");

                        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(key)) 
                            writer.WriteLine($"{key}: {value}");

                        writer.WriteLine("");
                    }

                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Log.Add("ConfigUtilities", e);
            }
        }
    }

    public static class ServerUtilities
    {
        public static void IssueOfflineBan(Timer.Time type, int duration, string userId, string issuer, string reason)
        {
            BanHandler.BanType banType = BanHandler.BanType.IP;

            if (ulong.TryParse(userId, out ulong id)) 
                banType = BanHandler.BanType.UserId;

            if (banType != BanHandler.BanType.IP)
            {
                UserIdType idType = id.ToString().Length == 17 ? UserIdType.Steam : UserIdType.Discord;
                userId = $"{id}@{idType.ToString().ToLower()}";
            }

            Log.Debug("SERVER", $"Issuing offline ban\nDuration: {duration} {type}\nUserID: {userId}\nIssuer: {issuer}\nReason: \"{reason}\"");

            switch (type)
            {
                case Timer.Time.Second:
                    {
                        long ticks = DateTime.Now.AddSeconds(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Minute:
                    {
                        long ticks = DateTime.UtcNow.AddMinutes(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Hour:
                    {
                        long ticks = DateTime.UtcNow.AddHours(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Day:
                    {
                        long ticks = DateTime.UtcNow.AddDays(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Month:
                    {
                        long ticks = DateTime.UtcNow.AddMonths(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Year:
                    {
                        long ticks = DateTime.UtcNow.AddYears(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                default:
                    return;
            }
        }

        public static bool AddReservedSlot(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || userId.StartsWith("#")) 
                    return false;

                if (!userId.Contains("@"))
                {
                    UserIdType userIdType = userId.Length == 17 ? UserIdType.Steam : UserIdType.Discord;

                    if (userIdType == UserIdType.Unspecified) 
                        return false;

                    userId += $"@{userIdType.ToString().ToLower()}";
                }

                if (!File.Exists(Directories.ReservedSlotsFile)) 
                    File.Create(Directories.ReservedSlotsFile).Close();

                using (StreamWriter writer = new StreamWriter(Directories.ReservedSlotsFile, true))
                {
                    writer.WriteLine("");
                    writer.WriteLine(userId);
                    writer.Flush();
                    writer.Close();
                    FileManager.RemoveEmptyLines(Directories.ReservedSlotsFile);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Add(nameof(AddReservedSlot), e);
                return false;
            }
        }
    }
}
