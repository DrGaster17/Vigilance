using UnityEngine;
using Mirror;
using Hints;
using System.Collections.Generic;
using Vigilance.Extensions;
using Vigilance.Enums;
using RemoteAdmin;
using System.Linq;
using CustomPlayerEffects;

namespace Vigilance.API
{
    public class Player
    {
        private ReferenceHub _hub;

        public Player(ReferenceHub hub)
        {
            _hub = hub;
            IsInvisible = false;
            PlayerLock = false;
            CanTriggerScp096 = true;
            CanBlockScp173 = true;
            Session = new Dictionary<string, object>();
        }

        public GameObject GameObject => _hub.gameObject;
        public ReferenceHub Hub => _hub;


        public Dictionary<string, object> Session { get; set; }
        public bool PlayerLock { get; set; }
        public bool CanTriggerScp096 { get; set; }
        public bool CanBlockScp173 { get; set; }
        public bool IsFriendlyFireEnabled { get; set; }
        public bool IsUsingStamina { get; set; }
        public bool IsInvisible { get; set; }


        public bool BypassMode { get => _hub.serverRoles.BypassMode; set => _hub.serverRoles.BypassMode = value; }
        public bool DoNotTrack { get => _hub.serverRoles.DoNotTrack; set => _hub.serverRoles.DoNotTrack = value; }
        public bool RemoteAdmin { get => _hub.serverRoles.RemoteAdmin; set => _hub.serverRoles.RemoteAdmin = value; }
        public bool GodMode { get => _hub.characterClassManager.GodMode; set => _hub.characterClassManager.GodMode = value; }
        public bool BadgeHidden { get => Utilities.Utils.GetBadgeHidden(_hub.serverRoles); set => Utilities.Utils.SetBadgeHidden(value, this); }
        public bool IsInOverwatch { get => _hub.serverRoles.OverwatchEnabled; set => _hub.serverRoles.SetOverwatchStatus(value); }
        public bool IsIntercomMuted { get => _hub.characterClassManager.NetworkIntercomMuted; set => _hub.characterClassManager.NetworkIntercomMuted = value; }
        public bool IsMuted { get => _hub.characterClassManager.NetworkMuted; set => _hub.characterClassManager.NetworkMuted = value; }
        public bool IsCuffed => _hub.handcuffs.NetworkCufferId != -1;
        public bool IsReloading => _hub.weaponManager.IsReloading();
        public bool IsZooming { get => _hub.weaponManager.NetworksyncZoomed; set => _hub.weaponManager.NetworksyncZoomed = value; }
        public bool IsFlashed { get => _hub.weaponManager.NetworksyncFlash; set => _hub.weaponManager.NetworksyncFlash = value; }
        public bool IsAlive => Role != RoleType.None && Role != RoleType.Spectator;
        public bool IsSCP => Team == TeamType.SCP;
        public bool IsNTF => Team == TeamType.NineTailedFox || Role == RoleType.FacilityGuard;
        public bool IsSprinting => _hub.fpc.IsSprinting;
        public bool IsSneaking => _hub.fpc.IsSneaking;
        public bool IsWalking => _hub.fpc.IsWalking;
        public bool IsJumping => _hub.fpc.isJumping;
        public bool IsInPocketDimension => Room != null && Room.Type == RoomType.PocketDimension;
        public bool IsHost => _hub.characterClassManager.IsHost;
        public bool IsDead => Team == TeamType.Spectator || Role == RoleType.None;
        public bool Is939 => _hub.characterClassManager.CurClass.Is939();
        public bool IsConnected => _hub.Ready && Role != RoleType.None;
        public int Health { get => (int)_hub.playerStats.Health; set => _hub.playerStats.SetHPAmount(value); }
        public int MaxHealth { get => _hub.playerStats.maxHP; set => _hub.playerStats.maxHP = value; }
        public int PlayerId { get => _hub.queryProcessor.PlayerId; set => _hub.queryProcessor.NetworkPlayerId = value; }
        public int CameraId { get => Camera.cameraId; set => Camera = Map.GetCamera(value); }
        public int CufferId { get => _hub.handcuffs.NetworkCufferId; set => _hub.handcuffs.NetworkCufferId = value; }
        public float Stamina { get => _hub.fpc.GetStamina(); set => _hub.fpc.ModifyStamina(value); }
        public ulong ParsedUserId => Utilities.Utils.GetParsedId(this);
        public string GroupNode => Utilities.Utils.GetGroupNode(UserGroup);
        public string IpAddress { get => _hub.queryProcessor._ipAddress; set => _hub.queryProcessor._ipAddress = value; }
        public string RankName { get => string.IsNullOrEmpty(_hub?.nicknameSync?.Network_myNickSync) ? "Dedicated Server" : (_hub.serverRoles.Group == null ? "Player" : _hub.serverRoles.Group.BadgeText); }
        public string CustomInfo { get => _hub.nicknameSync.Network_customPlayerInfoString; set => _hub.nicknameSync.Network_customPlayerInfoString = value; }
        public string Nick { get => string.IsNullOrEmpty(_hub?.nicknameSync?.MyNick) ? "Dedicated Server" : _hub.nicknameSync.MyNick; set => _hub.nicknameSync.SetNick(value); }
        public string DisplayNick { get => _hub.nicknameSync.Network_displayName; set => _hub.nicknameSync.Network_displayName = value; }
        public string Token { get => _hub.characterClassManager.AuthTokenSerial; set => _hub.characterClassManager.AuthTokenSerial = value; }
        public string UserId { get => string.IsNullOrEmpty(_hub.characterClassManager.UserId) ? "Dedicated Server" : _hub.characterClassManager.UserId; set => _hub.characterClassManager.UserId = value; }
        public string CustomUserId { get => _hub.characterClassManager.UserId2; set => _hub.characterClassManager.UserId2 = value; }
        public string NtfUnit { get => _hub.characterClassManager.NetworkCurUnitName; set => _hub.characterClassManager.NetworkCurUnitName = value; }
        public ServerRoles.AccessMode RemoteAdminAccessMode { get => _hub.serverRoles.RemoteAdminMode; set => _hub.serverRoles.RemoteAdminMode = value; }
        public ServerRoles.NamedColor RankColor { get => _hub.serverRoles.CurrentColor; set => _hub.serverRoles.SetColor(value.Name); }
        public UserGroup UserGroup { get => _hub.serverRoles.Group; set => _hub.serverRoles.SetGroup(value, false); }
        public PlayerCommandSender CommandSender => _hub.queryProcessor._sender;
        public Player Cuffer { get => Server.PlayerList.GetPlayer(CufferId); set => Handcuff(value); }
        public Room Room => Utilities.Utils.GetRoom(this);
        public List<Ragdoll> Ragdolls => Utilities.Utils.GetRagdolls(this);
        public List<Pickup> Pickups => Utilities.Utils.GetPickups(this);
        public Camera079 Camera { get => _hub.scp079PlayerScript.currentCamera; set => _hub.scp079PlayerScript.RpcSwitchCamera(value.cameraId, false); }
        public Transform PlayerCamera => _hub.PlayerCameraReference;
        public NetworkConnection Connection => _hub.scp079PlayerScript.connectionToClient;
        public NetworkIdentity ConnectionIdentity => Connection.identity;
        public Vector3 MovementDirectory => _hub.fpc.GetMoveDir;
        public Vector3 Scale { get => _hub.gameObject.transform.localScale; set => Utilities.Utils.SetScale(this, value); }
        public Vector3 Position { get => _hub.playerMovementSync.RealModelPosition; set => _hub.playerMovementSync.OverridePosition(value, _hub.PlayerCameraReference.rotation.y); }
        public Vector3 Rotation { get => _hub.PlayerCameraReference.forward; set => _hub.PlayerCameraReference.forward = value; }
        public Quaternion Rotations { get => _hub.PlayerCameraReference.rotation; set => _hub.PlayerCameraReference.rotation = value; }
        public Color RoleColor => Role.GetColor();
        public Inventory.SyncItemInfo CurrentItem { get => _hub.inventory.GetItemInHand(); set => _hub.inventory.items[_hub.inventory.items.IndexOf(_hub.inventory.GetItemInHand())] = value; }
        public ItemType ItemInHand { get => _hub.inventory.curItem; set => _hub.inventory.SetCurItem(value); }
        public RoleType Role { get => _hub.characterClassManager.NetworkCurClass; set => _hub.characterClassManager.SetPlayersClass(value, GameObject, false, false); }
        public PlayerInfoArea InfoArea { get => _hub.nicknameSync.Network_playerInfoToShow; set => _hub.nicknameSync.Network_playerInfoToShow = value; }
        public UserIdType UserIdType => Utilities.Utils.GetIdType(this);
        public TeamType Team => Role.GetTeam();


        public bool CheckPermission(PlayerPermissions perm) => PermissionsHandler.IsPermitted(UserGroup.Permissions, perm);
        public void Ban(int duration, string reason = "", string issuer = "") => Server.Ban(this, duration, reason, issuer);
        public void Kick(string reason = "") => Server.Kick(this, reason);
        public void IssuePermanentBan(string reason = "") => Server.IssuePermanentBan(this, reason);
        public void Broadcast(string message, int duration, bool monospaced) => API.Broadcast.LocalBroadcast.TargetAddElement(Connection, message, (ushort)duration, monospaced ? global::Broadcast.BroadcastFlags.Monospaced : global::Broadcast.BroadcastFlags.Normal);
        public void Broadcast(Broadcast bc) => Broadcast(bc.Message, bc.Duration, bc.Monospaced);
        public void ClearBroadcasts() => API.Broadcast.LocalBroadcast.TargetClearElements(Connection);
        public void ConsoleMessage(string message, string color = "green") => _hub.characterClassManager.TargetConsolePrint(Connection, message, color);
        public void RemoteAdminMessage(string message) => _hub.queryProcessor._sender.SendRemoteAdminMessage(message);
        public void Damage(int amount, DamageTypes.DamageType type = null) => _hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(amount, Nick, type == null ? DamageTypes.Falldown : type, PlayerId), GameObject, false);
        public void Kill() => Damage(int.MaxValue);
        public void Teleport(Vector3 pos) => _hub.playerMovementSync.OverridePosition(pos, _hub.PlayerCameraReference.rotation.y);
        public void Teleport(RoomType room) => Teleport(Map.GetRoom(room).Position);
        public void Teleport(string roomName) => Teleport(Map.GetRoom(roomName).Position);
        public void RemoveHeldItem() => _hub.inventory.items.Remove(_hub.inventory.GetItemInHand());
        public void RemoveItem(Inventory.SyncItemInfo item) => _hub.inventory.items.Remove(item);
        public void ClearInventory() => _hub.inventory.Clear();
        public void AddItem(ItemType item) => Utilities.Utils.AddItem(this, item);
        public void ResetInventory(List<Inventory.SyncItemInfo> newItems) => ResetInventory(newItems.Select(item => item.id).ToList());
        public void DropAllItems() => Hub.inventory.ServerDropAll();
        public int GetAmmo(AmmoType ammoType) => (int)_hub.ammoBox[(int)ammoType];
        public T GetComponent<T>() where T : Component => _hub.gameObject.GetComponent<T>();
        public T AddComponent<T>() where T : Component => _hub.gameObject.AddComponent<T>();
        public bool HasItem(ItemType item) => _hub.inventory.items.Select(h => h.id).Contains(item);
        public void ResetStamina() => _hub.fpc.ResetStamina();
        public void ShowHint(Broadcast bc) => ShowHint(bc.Message, bc.Duration);
        public void OpenRemoteAdmin() => _hub.serverRoles.CallTargetOpenRemoteAdmin(Connection, string.IsNullOrEmpty(ServerStatic.GetPermissionsHandler()?._overridePassword) ? false : true);
        public void OpenRemoteAdmin(bool password = false) => _hub.serverRoles.CallTargetOpenRemoteAdmin(Connection, password);
        public void CloseRemoteAdmin() => _hub.serverRoles.CallTargetCloseRemoteAdmin(Connection);
        public void SpawnGrenade(GrenadeType type) => Utilities.Utils.SpawnGrenade(this, type);
        public void SpawnDummy(RoleType role, Vector3 scale) => Utilities.Utils.SpawnDummyModel(Position, Rotations, role, scale.x, scale.y, scale.z);

        public void ShowHint(string message, float duration = 10f)
        {
            HintParameter[] parameters = new HintParameter[]
            {
                new StringHintParameter(message),
            };
            _hub.hints.Show(new TextHint(message, parameters, null, duration));
        }

        public void DropItem(Inventory.SyncItemInfo item)
        {
            _hub.inventory.SetPickup(item.id, item.durability, Position, _hub.inventory.camera.transform.rotation, item.modSight, item.modBarrel, item.modOther);
            _hub.inventory.items.Remove(item);
        }

        public void SetRole(RoleType newRole, bool keepPosition = false, bool isEscaped = false)
        {
            _hub.characterClassManager.SetPlayersClass(newRole, GameObject, keepPosition, isEscaped);
        }

        public void Handcuff(Player cuffer)
        {
            if (cuffer == null)
            {
                Hub.handcuffs.NetworkForceCuff = true;
                return;
            }

            if (!IsCuffed && cuffer.Hub.inventory.items.Any(item => item.id == ItemType.Disarmer) && Vector3.Distance(Position, cuffer.Position) <= 130f)
            {
                CufferId = cuffer.PlayerId;
            }
        }

        public void Uncuff()
        {
            Hub.handcuffs.NetworkForceCuff = false;
            Hub.handcuffs.CufferId = -1;
        }

        public void ResetInventory(List<ItemType> newItems)
        {
            _hub.inventory.Clear();
            if (newItems != null && newItems.Count > 0)
            {
                foreach (ItemType item in newItems)
                    AddItem(item);
            }
        }

        public void SetInfo(PlayerInfoArea area, string content)
        {
            _hub.nicknameSync.ShownPlayerInfo = area;
            _hub.nicknameSync.Network_customPlayerInfoString = content;
        }

        public void SetAmmo(Inventory.SyncItemInfo weapon, int ammo)
        {
            _hub.inventory.items.ModifyDuration(_hub.inventory.items.IndexOf(weapon), ammo);
        }

        public void SetAttachments(Inventory.SyncItemInfo item, int sight, int barrel, int other)
        {
            _hub.inventory.items.ModifyAttachments(_hub.inventory.items.IndexOf(item), sight, barrel, other);
        }

        public void SetAmmo(WeaponType weapon, int ammo, bool iterate = true)
        {
            if (!iterate)
            {
                Inventory.SyncItemInfo item = GetWeapons(weapon).FirstOrDefault();
                if (item == default)
                    return;
                SetAmmo(item, ammo);
                return;
            }

            foreach (Inventory.SyncItemInfo item in GetWeapons(weapon))
            {
                SetAmmo(item, ammo);
            }
        }

        public void SetAmmo(AmmoType ammo, int amount)
        {
            Hub.ammoBox[(int)ammo] = (uint)amount;
        }

        public float GetAmmo(Inventory.SyncItemInfo weapon)
        {
            return weapon.durability;
        }

        public float GetAmmo(WeaponType weapon)
        {
            Inventory.SyncItemInfo item = GetWeapon(weapon);
            return item.durability;
        }

        public void Explode(float force)
        {
            for (int i = 0; i < force; i++)
            {
                Prefab.GrenadeFrag.Spawn(Position, Rotations, Vector3.one);
            }

            float distance = 5f + force;
            float doorDistance = 10f + force;

            foreach (Player player in Server.Players)
            {
                if (player.Distance(Position) <= distance && !player.GodMode && player.IsAlive)
                {
                    _hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(21212f, Nick, DamageTypes.Nuke, PlayerId), player.GameObject);
                }
            }
        }

        public void Achieve(Achievement achievement)
        {
            if (achievement == Achievement.Unknown)
                return;
            if (achievement == Achievement.JustResources)
            {
                Utilities.Utils.LocalStats?.TargetStats(Connection, "dboys_killed", "justresources", 50);
                return;
            }
            Utilities.Utils.LocalStats?.TargetAchieve(Connection, achievement.ToString().ToLower());
        }

        public float Distance(Vector3 pos) => Vector3.Distance(Position, pos);

        public Inventory.SyncItemInfo GetWeapon(WeaponType weapon)
        {
            foreach (Inventory.SyncItemInfo item in Hub.inventory.items)
            {
                if (item.id == ItemType.GunCOM15 && weapon == WeaponType.Com15)
                    return item;
                if (item.id == ItemType.GunE11SR && weapon == WeaponType.Epsilon11)
                    return item;
                if (item.id == ItemType.GunLogicer && weapon == WeaponType.Logicer)
                    return item;
                if (item.id == ItemType.GunMP7 && weapon == WeaponType.MP7)
                    return item;
                if (item.id == ItemType.GunProject90 && weapon == WeaponType.Project90)
                    return item;
                if (item.id == ItemType.GunUSP && weapon == WeaponType.USP)
                    return item;
                if (item.id == ItemType.MicroHID && weapon == WeaponType.MicroHID)
                    return item;
            }
            return default;
        }

        public List<Inventory.SyncItemInfo> GetWeapons(WeaponType weapon)
        {
            List<Inventory.SyncItemInfo> items = new List<Inventory.SyncItemInfo>();
            foreach (Inventory.SyncItemInfo item in Hub.inventory.items)
            {
                if (item.id == ItemType.GunCOM15 && weapon == WeaponType.Com15)
                    items.Add(item);
                if (item.id == ItemType.GunE11SR && weapon == WeaponType.Epsilon11)
                    items.Add(item);
                if (item.id == ItemType.GunLogicer && weapon == WeaponType.Logicer)
                    items.Add(item);
                if (item.id == ItemType.GunMP7 && weapon == WeaponType.MP7)
                    items.Add(item);
                if (item.id == ItemType.GunProject90 && weapon == WeaponType.Project90)
                    items.Add(item);
                if (item.id == ItemType.GunUSP && weapon == WeaponType.USP)
                    items.Add(item);
                if (item.id == ItemType.MicroHID && weapon == WeaponType.MicroHID)
                    items.Add(item);
            }
            return items;
        }

        public Sight GetSight(Inventory.SyncItemInfo weapon)
        {
            if (weapon == default)
                return Sight.None;
            WeaponManager wmanager = Hub.weaponManager;
            if (weapon.id.IsWeapon())
            {
                WeaponManager.Weapon wep = wmanager.weapons.Where(wp => wp.inventoryID == weapon.id).FirstOrDefault();
                if (wep != null)
                {
                    switch (wep.mod_sights[weapon.modSight].name)
                    {
                        case "Collimator":
                            return Sight.Collimator;
                        case "Holo Sight":
                            return Sight.Holo;
                        case "Red Dot":
                            return Sight.RedDot;
                        case "Blue Dot Sight":
                            return Sight.BlueDot;
                        case "Night Vision Sight":
                            return Sight.NightVision;
                        case "Sniper Scope":
                            return Sight.SniperScope;
                    }
                }
            }
            return Sight.None;
        }

        public Sight GetSight(WeaponType weapon) => GetSight(GetWeapon(weapon));

        public void SetSight(Sight sight, Inventory.SyncItemInfo weapon)
        {
            if (weapon == default)
                return;
            WeaponManager wmanager = Hub.weaponManager;
            if (weapon.id.IsWeapon())
            {
                WeaponManager.Weapon wep = wmanager.weapons.Where(wp => wp.inventoryID == weapon.id).FirstOrDefault();
                if (wep != null)
                {
                    string name = "None";
                    switch (sight)
                    {
                        case Sight.Collimator:
                            name = "Collimator";
                            break;
                        case Sight.Holo:
                            name = "Holo Sight";
                            break;
                        case Sight.BlueDot:
                            name = "Blue Dot Sight";
                            break;
                        case Sight.RedDot:
                            name = "Red Dot";
                            break;
                        case Sight.NightVision:
                            name = "Night Vision Sight";
                            break;
                        case Sight.SniperScope:
                            name = "Sniper Scope";
                            break;
                    }

                    int weaponMod = wep.mod_sights.Select((s, i) => new { s, i }).Where(e => e.s.name == name).Select(e => e.i).FirstOrDefault();
                    int weaponId = Hub.inventory.items.FindIndex(s => s == weapon);
                    weapon.modSight = weaponMod;
                    Hub.inventory.items[weaponId] = weapon;
                }
            }
        }

        public void SetSight(WeaponType weapon, Sight sight) => SetSight(sight, GetWeapon(weapon));

        public Barrel GetBarrel(Inventory.SyncItemInfo weapon)
        {
            if (weapon == default)
                return Barrel.None;
            WeaponManager wmanager = Hub.weaponManager;
            if (weapon.id.IsWeapon())
            {
                WeaponManager.Weapon wep = wmanager.weapons.Where(wp => wp.inventoryID == weapon.id).FirstOrDefault();
                if (wep != null)
                {
                    switch (wep.mod_barrels[weapon.modBarrel].name)
                    {
                        case "Suppressor":
                            return Barrel.Suppressor;
                        case "Silencer":
                            return Barrel.Silencer;
                        case "Muzzle Brake":
                            return Barrel.MuzzleBrake;
                        case "Heavy Barrel":
                            return Barrel.HeavyBarrel;
                        case "Muzzle Booster":
                            return Barrel.MuzzleBooster;
                    }
                }
            }
            return Barrel.None;
        }

        public Barrel GetBarrel(WeaponType weapon) => GetBarrel(GetWeapon(weapon));

        public void SetBarrel(Barrel barrel, Inventory.SyncItemInfo weapon)
        {
            if (weapon == default)
                return;
            WeaponManager wmanager = Hub.weaponManager;
            if (weapon.id.IsWeapon())
            {
                WeaponManager.Weapon wep = wmanager.weapons.Where(wp => wp.inventoryID == weapon.id).FirstOrDefault();
                if (wep != null)
                {
                    string name = "None";
                    switch (barrel)
                    {
                        case Barrel.HeavyBarrel:
                            name = "Heavy Barrel";
                            break;
                        case Barrel.MuzzleBooster:
                            name = "Muzzle Booster";
                            break;
                        case Barrel.MuzzleBrake:
                            name = "Muzzle Brake";
                            break;
                        case Barrel.Silencer:
                            name = "Silencer";
                            break;
                        case Barrel.Suppressor:
                            name = "Suppressor";
                            break;
                    }

                    int weaponMod = wep.mod_barrels.Select((s, i) => new { s, i }).Where(e => e.s.name == name).Select(e => e.i).FirstOrDefault();
                    int weaponId = Hub.inventory.items.FindIndex(s => s == weapon);
                    weapon.modBarrel = weaponMod;
                    Hub.inventory.items[weaponId] = weapon;
                }
            }
        }

        public void SetBarrel(Barrel barell, WeaponType weapon) => SetBarrel(barell, GetWeapon(weapon));

        public Other GetOther(Inventory.SyncItemInfo weapon)
        {
            if (weapon == default)
                return Other.None;
            WeaponManager wmanager = Hub.weaponManager;
            if (weapon.id.IsWeapon())
            {
                WeaponManager.Weapon wep = wmanager.weapons.Where(wp => wp.inventoryID == weapon.id).FirstOrDefault();
                if (wep != null)
                {
                    switch (wep.mod_others[weapon.modOther].name)
                    {
                        case "Flashlight":
                            return Other.Flashlight;
                        case "Ammo Counter":
                            return Other.AmmoCounter;
                        case "Gyroscopic Stabilizer":
                            return Other.GyroscopicStabilizer;
                        case "Laser":
                            return Other.Laser;
                    }
                }
            }
            return Other.None;
        }

        public Other GetOther(WeaponType weapon) => GetOther(GetWeapon(weapon));

        public void SetOther(Other other, Inventory.SyncItemInfo weapon)
        {
            if (weapon == default)
                return;
            WeaponManager wmanager = Hub.weaponManager;
            if (weapon.id.IsWeapon())
            {
                WeaponManager.Weapon wep = wmanager.weapons.Where(wp => wp.inventoryID == weapon.id).FirstOrDefault();
                if (wep != null)
                {
                    string name = "None";
                    switch (other)
                    {
                        case Other.AmmoCounter:
                            name = "Ammo Counter";
                            break;
                        case Other.Flashlight:
                            name = "Flashlight";
                            break;
                        case Other.GyroscopicStabilizer:
                            name = "Gyroscopic Stabilizer";
                            break;
                        case Other.Laser:
                            name = "Laser";
                            break;
                    }

                    int weaponMod = wep.mod_others.Select((s, i) => new { s, i }).Where(e => e.s.name == name).Select(e => e.i).FirstOrDefault();
                    int weaponId = Hub.inventory.items.FindIndex(s => s == weapon);
                    weapon.modOther = weaponMod;
                    Hub.inventory.items[weaponId] = weapon;
                }
            }
        }

        public void SetOther(Other other, WeaponType weapon) => SetOther(other, GetWeapon(weapon));

        public void CreatePortal(Player target = null)
        {
            if (Role != RoleType.Scp106)
                return;
            if (target == null)
            {
                _hub.scp106PlayerScript.CallCmdMakePortal();
            }
            else
            {
                Scp106PlayerScript script = _hub.scp106PlayerScript;
                Transform transform = target.GameObject.transform;
                Debug.DrawRay(transform.position, -transform.up, Color.red, 10f);
                RaycastHit raycastHit;
                if (script.iAm106 && !script.goingViaThePortal && Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out raycastHit, 10f, script.teleportPlacementMask))
                {
                    Vector3 pos = raycastHit.point - Vector3.up;
                    script.SetPortalPosition(pos);
                }
            }
        }

        public void CreatePortal(Vector3 pos)
        {
            if (Role != RoleType.Scp106)
                return;
            if (pos == Vector3.zero)
            {
                _hub.scp106PlayerScript.CallCmdMakePortal();
            }
            else
            {
                Scp106PlayerScript script = _hub.scp106PlayerScript;
                Debug.DrawRay(pos, -Vector3.up, Color.red, 10f);
                RaycastHit raycastHit;
                if (script.iAm106 && !script.goingViaThePortal && Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out raycastHit, 10f, script.teleportPlacementMask))
                {
                    Vector3 position = raycastHit.point - Vector3.up;
                    script.SetPortalPosition(position);
                }
            }
        }

        public void UsePortal()
        {
            if (Role != RoleType.Scp106)
                return;
            _hub.scp106PlayerScript.CallCmdUsePortal();
        }

        public void Teleport(Room room) => Teleport(Utilities.Utils.FindSafePosition(room.Position));
        public void Teleport(Rid rid) => Teleport(Utilities.Utils.FindSafePosition(rid.transform.position));

        public void EnableEffect<T>(float duration = 0f, bool addIfActive = false) where T : PlayerEffect => _hub.playerEffectsController.EnableEffect<T>(duration, addIfActive);
        public void DisableEffect<T>() where T : PlayerEffect => _hub.playerEffectsController.DisableEffect<T>();
        public T GetEffect<T>() where T : PlayerEffect => _hub.playerEffectsController.GetEffect<T>();
        public override string ToString() => $"[{PlayerId}]: {(RemoteAdmin ? "[RA]" : "")} {Nick} [{UserId}] [{IpAddress}] [{Role.AsString()}]";
    }
}
