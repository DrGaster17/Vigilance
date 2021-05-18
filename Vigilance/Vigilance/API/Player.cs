using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vigilance.API.Enums;
using Mirror;
using Hints;
using RemoteAdmin;
using CustomPlayerEffects;
using Vigilance.Utilities;
using Vigilance.Extensions;
using Vigilance.Extensions.Rpc;
using Grenades;
using PlayableScps;

namespace Vigilance.API
{
    public class Player
    {
        private ReferenceHub _refHub;
        private IEnumerable<CommandPermission> _perms;

        public Player(ReferenceHub hub)
        {
            _refHub = hub;
            _perms = PluginManager.Config.Permissions.TryGetValue(UserId, out List<CommandPermission> perms) ? perms : new List<CommandPermission>() { CommandPermission.None }; 
        }

        public Player(GameObject obj)
        {
            _refHub = ReferenceHub.GetHub(obj);
            _perms = PluginManager.Config.Permissions.TryGetValue(UserId, out List<CommandPermission> perms) ? perms : new List<CommandPermission>() { CommandPermission.None };
        }

        public Player(int playerId)
        {
            _refHub = ReferenceHub.GetHub(playerId);
            _perms = PluginManager.Config.Permissions.TryGetValue(UserId, out List<CommandPermission> perms) ? perms : new List<CommandPermission>() { CommandPermission.None };
        }

        public Player(string userId)
        {
            _refHub = ReferenceHub.GetAllHubs().Values.Where(h => h.characterClassManager.UserId == userId).FirstOrDefault();
            _perms = PluginManager.Config.Permissions.TryGetValue(UserId, out List<CommandPermission> perms) ? perms : new List<CommandPermission>() { CommandPermission.None };
        }

        public Dictionary<string, object> Session { get; } = new Dictionary<string, object>();
        public List<Player> TargetGhosts { get; } = new List<Player>();
        public IEnumerable<CommandPermission> Permissions { get => _perms; set => _perms = value; }

        public bool IsInvisible { get; set; }
        public bool PlayerLock { get; set; }

        public bool IsHost => _refHub == null || _refHub.characterClassManager == null || _refHub.queryProcessor == null
                                              || string.IsNullOrEmpty(_refHub.characterClassManager.UserId) || _refHub.queryProcessor.PlayerId == 0
                                              || _refHub.characterClassManager.IsHost || _refHub.isLocalPlayer || _refHub.isDedicatedServer;

        public bool BypassMode { get => _refHub.serverRoles.BypassMode; set => _refHub.serverRoles.BypassMode = value; }
        public bool DoNotTrack { get => _refHub.serverRoles.DoNotTrack; set => _refHub.serverRoles.DoNotTrack = value; }
        public bool RemoteAdmin { get => _refHub.serverRoles.RemoteAdmin; set => _refHub.serverRoles.RemoteAdmin = value; }
        public bool GodMode { get => _refHub.characterClassManager.GodMode; set => _refHub.characterClassManager.GodMode = value; }
        public bool BadgeHidden { get => !string.IsNullOrEmpty(_refHub.serverRoles.HiddenBadge); }
        public bool IsInOverwatch { get => _refHub.serverRoles.OverwatchEnabled; set => _refHub.serverRoles.SetOverwatchStatus(value); }
        public bool IsIntercomMuted { get => _refHub.characterClassManager.NetworkIntercomMuted; set => _refHub.characterClassManager.NetworkIntercomMuted = value; }
        public bool IsMuted { get => _refHub.characterClassManager.NetworkMuted; set => _refHub.characterClassManager.NetworkMuted = value; }
        public bool IsCuffed => _refHub.handcuffs.NetworkCufferId != -1;
        public bool IsReloading => _refHub.weaponManager.IsReloading();
        public bool IsZooming { get => _refHub.weaponManager.NetworksyncZoomed; set => _refHub.weaponManager.NetworksyncZoomed = value; }
        public bool IsFlashed { get => _refHub.weaponManager.NetworksyncFlash; set => _refHub.weaponManager.NetworksyncFlash = value; }
        public bool IsAlive => Role != RoleType.None && Role != RoleType.Spectator;
        public bool IsSCP => Team == Team.SCP;
        public bool IsNTF => Team == Team.MTF || Role == RoleType.FacilityGuard;
        public bool IsSprinting => _refHub.fpc.IsSprinting;
        public bool IsSneaking => _refHub.fpc.IsSneaking;
        public bool IsWalking => _refHub.fpc.IsWalking;
        public bool IsJumping => _refHub.fpc.isJumping;
        public bool IsInPocketDimension => Room != null && Room.Zone == ZoneType.PocketDimension;
        public bool IsDead => Team == Team.RIP || Role == RoleType.None;
        public bool Is939 => _refHub.characterClassManager.CurClass.Is939();
        public bool IsConnected => _refHub.Ready && Role != RoleType.None && _refHub.gameObject != null;

        public int MaxArtificalHealth { get => _refHub.playerStats.NetworkmaxArtificialHealth; set => _refHub.playerStats.NetworkmaxArtificialHealth = value; }
        public int CurrentAnimation { get => _refHub.animationController.curAnim; }
        public int EscapeStartTime { get => _refHub.characterClassManager.EscapeStartTime; }
        public int Health { get => (int)_refHub.playerStats.Health; set => _refHub.playerStats.SetHPAmount(value); }
        public int MaxHealth { get => _refHub.playerStats.maxHP; set => _refHub.playerStats.maxHP = value; }
        public int PlayerId { get => _refHub.queryProcessor.NetworkPlayerId; set => _refHub.queryProcessor.NetworkPlayerId = value; }
        public int CameraId { get => Camera == null ? 0 : Camera.cameraId; set => _refHub.scp079PlayerScript.RpcSwitchCamera((ushort)value, false); }
        public int CufferId { get => _refHub.handcuffs.NetworkCufferId; set => _refHub.handcuffs.NetworkCufferId = value; }
        public int CurrentItemIndex { get => _refHub.inventory.GetItemIndex(); }

        public float AfkTime { get => _refHub.playerMovementSync.AFKTime; set => _refHub.playerMovementSync.AFKTime = value; }
        public float Stamina { get => _refHub.fpc.GetStamina(); set => _refHub.fpc.ModifyStamina(value); }
        public float MaxEnergy { get => _refHub.scp079PlayerScript.NetworkmaxMana; set => PlayerUtilities.SetMaxEnergy(this, value); }
        public float Energy { get => _refHub.scp079PlayerScript.NetworkcurMana; set => PlayerUtilities.SetEnergy(this, value); }
        public float Experience { get => _refHub.scp079PlayerScript.NetworkcurExp; set => PlayerUtilities.SetExperience(this, value); }

        public float RunSpeedMultiplier { get => ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier; set => this.ChangeRunningSpeed(value, false); }
        public float WalkSpeedMultiplier { get => ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier; set => this.ChangeWalkingSpeed(value, false); }

        public float AliveTime { get => _refHub.characterClassManager.AliveTime; }
        public float ArtificialHealth { get => _refHub.playerStats.NetworksyncArtificialHealth; set => _refHub.playerStats.NetworksyncArtificialHealth = value; }
        public float ArtificialHealthDecay { get => _refHub.playerStats.NetworkartificialHpDecay; set => _refHub.playerStats.NetworkartificialHpDecay = value; }
        public float ArtificialNormalRatio { get => _refHub.playerStats.NetworkartificialNormalRatio; set => _refHub.playerStats.NetworkartificialNormalRatio = value; }

        public byte Level { get => _refHub.scp079PlayerScript.NetworkcurLvl; set => PlayerUtilities.SetLevel(this, value); }

        public ulong ParsedUserId => PlayerUtilities.GetParsedId(this);

        public long DeathTime { get => _refHub.characterClassManager.DeathTime; }

        public string Asn { get => _refHub.characterClassManager.Asn; }
        public string GroupNode => PlayerUtilities.GetGroupNode(UserGroup);
        public string IpAddress { get => _refHub.queryProcessor._ipAddress; set => _refHub.queryProcessor._ipAddress = value; }
        public string RankName { get => string.IsNullOrEmpty(_refHub?.nicknameSync?.Network_myNickSync) ? "Dedicated Server" : (_refHub.serverRoles.Group == null ? "Player" : _refHub.serverRoles.Group.BadgeText); }
        public string CustomInfo { get => _refHub.nicknameSync.Network_customPlayerInfoString; set => _refHub.nicknameSync.Network_customPlayerInfoString = value; }
        public string Nick { get => string.IsNullOrEmpty(_refHub?.nicknameSync?.Network_myNickSync) ? "Dedicated Server" : _refHub.nicknameSync.Network_myNickSync; set => _refHub.nicknameSync.SetNick(value); }
        public string DisplayNick { get => _refHub.nicknameSync.Network_displayName; set => _refHub.nicknameSync.Network_displayName = value; }
        public string TokenSerial { get => _refHub.characterClassManager.AuthTokenSerial; }
        public string Token { get => _refHub.characterClassManager.AuthToken; }
        public string UserId { get => string.IsNullOrEmpty(_refHub.characterClassManager.UserId) ? "Dedicated Server" : _refHub.characterClassManager.UserId; set => _refHub.characterClassManager.UserId = value; }
        public string CustomUserId { get => _refHub.characterClassManager.UserId2; set => _refHub.characterClassManager.UserId2 = value; }
        public string NtfUnit { get => _refHub.characterClassManager.NetworkCurUnitName; set => _refHub.characterClassManager.NetworkCurUnitName = value; }

        public ServerRoles.AccessMode RemoteAdminAccessMode { get => _refHub.serverRoles.RemoteAdminMode; set => _refHub.serverRoles.RemoteAdminMode = value; }
        public ServerRoles.NamedColor RankColor { get => _refHub.serverRoles.CurrentColor; set => _refHub.serverRoles.SetColor(value.Name); }

        public UserGroup UserGroup { get => _refHub.serverRoles.Group; set => _refHub.serverRoles.SetGroup(value, false); }

        public PlayerCommandSender CommandSender => _refHub.queryProcessor._sender;
        public PlayableScp CurrentScp { get => _refHub.scpsController.CurrentScp; set => _refHub.scpsController.CurrentScp = value; }

        public Player Cuffer { get => PlayersList.GetPlayer(CufferId); set => Handcuff(value); }

        public Room Room => PlayerUtilities.GetRoom(this);

        public List<Ragdoll> Ragdolls => PlayerUtilities.GetRagdolls(this);
        public List<Pickup> Pickups => PlayerUtilities.GetPickups(this);
        public List<RoleType> Roles { get; } = new List<RoleType>();

        public IEnumerable<Inventory.SyncItemInfo> Items => _refHub.inventory.items;

        public Camera079 Camera { get => _refHub.scp079PlayerScript.currentCamera; set => _refHub.scp079PlayerScript.RpcSwitchCamera(value.cameraId, false); }
        public Transform PlayerCamera => _refHub.PlayerCameraReference;

        public NetworkConnection Connection => _refHub.scp079PlayerScript.connectionToClient;
        public NetworkIdentity ConnectionIdentity => Connection.identity;

        public GameObject GameObject => _refHub.gameObject;
        public GameObject LookingAt => Physics.Raycast(Position + PlayerCamera.forward, PlayerCamera.forward, out RaycastHit hit) ? hit.collider.gameObject : null;

        public ReferenceHub ReferenceHub => _refHub;

        public Vector3 Scale { get => _refHub.gameObject.transform.localScale; set => PlayerUtilities.SetScale(this, value); }
        public Vector3 Position { get => _refHub.playerMovementSync.RealModelPosition; set => _refHub.playerMovementSync.OverridePosition(value, _refHub.PlayerCameraReference.rotation.y); }
        public Vector3 Rotation { get => _refHub.PlayerCameraReference.forward; set => _refHub.PlayerCameraReference.forward = value; }
        public Vector3 DeathPosition { get => _refHub.characterClassManager.NetworkDeathPosition; set => _refHub.characterClassManager.NetworkDeathPosition = value; }
        public Vector3 Velocity { get => _refHub.playerMovementSync.PlayerVelocity; set => _refHub.playerMovementSync.PlayerVelocity = value; }
        public Quaternion Rotations { get => _refHub.PlayerCameraReference.rotation; set => _refHub.PlayerCameraReference.rotation = value; }
        public Inventory.SyncItemInfo CurrentItem { get => _refHub.inventory.GetItemInHand(); set => _refHub.inventory.items[CurrentItemIndex] = value; }

        public ItemType CurrentItemId { get => _refHub.inventory.curItem; set => _refHub.inventory.curItem = value; }
        public RoleType Role { get => _refHub.characterClassManager.NetworkCurClass; set => SetRole(value, true, false); }
        public PlayerInfoArea InfoArea { get => _refHub.nicknameSync.Network_playerInfoToShow; set => _refHub.nicknameSync.Network_playerInfoToShow = value; }
        public PlayerMovementState MovementState { get => _refHub.animationController.MoveState; }
        public UserIdType UserIdType => PlayerUtilities.GetIdType(this);
        public Team Team => _refHub.characterClassManager.CurRole.team;
        public Fraction Fraction => _refHub.characterClassManager.Fraction;

        public bool CheckPermission(PlayerPermissions perm) => PermissionsHandler.IsPermitted(UserGroup.Permissions, perm);
        public bool CheckPermission(CommandPermission perm) => _perms.Contains(perm);
        public bool HasItem(ItemType item) => _refHub.inventory.items.Select(h => h.id).Contains(item);

        public int GetAmmo(AmmoType ammoType) => (int)_refHub.ammoBox[(int)ammoType];

        public void Ban(int duration, string reason = "", string issuer = "") => Server.Ban(this, duration, reason, issuer);
        public void Kick(string reason = "") => Server.Kick(this, reason);
        public void IssuePermanentBan(string reason = "") => Server.IssuePermanentBan(this, reason);

        public void Broadcast(string message, int duration, bool monospaced) => CompCache.ReferenceHub.GetComponent<global::Broadcast>().TargetAddElement(Connection, message, (ushort)duration, monospaced ? global::Broadcast.BroadcastFlags.Monospaced : global::Broadcast.BroadcastFlags.Normal);
        public void Broadcast(Broadcast bc) => Broadcast(bc.Message, bc.Duration, bc.Monospaced);

        public void ClearBroadcasts() => CompCache.ReferenceHub.GetComponent<global::Broadcast>().TargetClearElements(Connection);
        public void ConsoleMessage(string message, string color = "green") => _refHub.characterClassManager.TargetConsolePrint(Connection, message, color);
        public void RemoteAdminMessage(string message) => _refHub.queryProcessor._sender.SendRemoteAdminMessage(message);

        public void Damage(int amount, DamageTypes.DamageType type = null) => _refHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(amount, Nick, type == null ? DamageTypes.Falldown : type, PlayerId), _refHub.gameObject, false);
        public void Kill() => Damage(int.MaxValue);

        public void Teleport(Vector3 pos) => _refHub.playerMovementSync.OverridePosition(pos, _refHub.PlayerCameraReference.rotation.y);

        public void ResetInventory(List<Inventory.SyncItemInfo> newItems) => ResetInventory(newItems.Select(item => item.id).ToList());
        public void ClearInventory() => _refHub.inventory.Clear();

        public void AddItem(ItemType item) => PlayerUtilities.AddItem(this, item);
        public void DropAllItems() => _refHub.inventory.ServerDropAll();
        public void DropHeldItem() => DropItem(CurrentItem);
        public void RemoveHeldItem() => _refHub.inventory.items.Remove(_refHub.inventory.GetItemInHand());
        public void RemoveItem(Inventory.SyncItemInfo item) => _refHub.inventory.items.Remove(item);


        public TComponent GetComponent<TComponent>() where TComponent : Component => _refHub.gameObject.GetComponent<TComponent>();
        public TComponent AddComponent<TComponent>() where TComponent : Component => _refHub.gameObject.AddComponent<TComponent>();

        public TComponent GetOrAddComponent<TComponent>() where TComponent: Component
        {
            if (!_refHub.gameObject.TryGetComponent(out TComponent component))
                component = _refHub.gameObject.AddComponent<TComponent>();

            return component;
        }

        public void ResetStamina() => _refHub.fpc.ResetStamina();
        public void ShowHint(Broadcast bc) => ShowHint(bc.Message, bc.Duration);

        public void OpenRemoteAdmin() => _refHub.serverRoles.TargetOpenRemoteAdmin(Connection, string.IsNullOrEmpty(Server.Password) ? false : true);
        public void OpenRemoteAdmin(bool password = false) => _refHub.serverRoles.TargetOpenRemoteAdmin(Connection, password);
        public void CloseRemoteAdmin() => _refHub.serverRoles.TargetCloseRemoteAdmin(Connection);

        public Grenade SpawnGrenade(GrenadeType type) => MapUtilities.SpawnGrenade(this, type);
        public GameObject SpawnDummy(RoleType role, Vector3 scale) => MapUtilities.SpawnDummyModel(Position, Rotations, role, scale.x, scale.y, scale.z);

        public Pickup DropItem(Inventory.SyncItemInfo item)
        {
            Pickup pickup = Map.SpawnItem(item, Position, Rotations);
            _refHub.inventory.items.Remove(item);
            return pickup;
        }

        public void ShowHint(string message, float duration = 10f)
        {
            HintParameter[] parameters = new HintParameter[]
            {
                new StringHintParameter(message),
            };

            _refHub.hints.Show(new TextHint(message, parameters, null, duration));
        }

        public void SetRole(RoleType newRole, bool keepPosition = false, bool isEscaped = false)
        {
            _refHub.characterClassManager.SetPlayersClass(newRole, _refHub.gameObject, keepPosition, isEscaped);
        }

        public void Handcuff(Player cuffer = null)
        {
            if (cuffer == null)
            {
                _refHub.handcuffs.NetworkForceCuff = true;
                return;
            }

            if (!IsCuffed && cuffer.CurrentItemId == ItemType.Disarmer && Vector3.Distance(Position, cuffer.Position) <= 130f)
            {
                CufferId = cuffer.PlayerId;
            }
        }

        public void Uncuff()
        {
            _refHub.handcuffs.NetworkForceCuff = false;
            _refHub.handcuffs.CufferId = -1;
        }

        public void ResetInventory(List<ItemType> newItems)
        {
            _refHub.inventory.Clear();

            if (newItems != null && newItems.Count > 0)
            {
                foreach (ItemType item in newItems)
                {
                    AddItem(item);
                }
            }
        }

        public void SetAmmo(Inventory.SyncItemInfo weapon, int ammo)
        {
            _refHub.inventory.items.ModifyDuration(_refHub.inventory.items.IndexOf(weapon), ammo);
        }

        public void SetAttachments(Inventory.SyncItemInfo item, int sight, int barrel, int other)
        {
            _refHub.inventory.items.ModifyAttachments(_refHub.inventory.items.IndexOf(item), sight, barrel, other);
        }

        public void SetAmmo(AmmoType ammo, int amount)
        {
            _refHub.ammoBox[(int)ammo] = (uint)amount;
        }

        public float GetAmmo(Inventory.SyncItemInfo weapon)
        {
            return weapon.durability;
        }

        public void Explode(float force)
        {
            for (int i = 0; i < force; i++)
            {
                PrefabType.GrenadeFrag.Spawn(Position, Rotations, Vector3.one);
            }

            float distance = 5f + force;
            float doorDistance = 10f + force;

            foreach (Player player in PlayersList.List)
            {
                if (player.Distance(Position) <= distance && !player.GodMode && player.IsAlive)
                {
                    _refHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(21212f, Nick, DamageTypes.Nuke, PlayerId), player.GameObject);
                }
            }

            foreach (Door door in Map.Doors)
            {
                if (Distance(door.Position) <= doorDistance)
                {
                    door.Destroy();
                }
            }
        }

        public float Distance(Vector3 pos) => Vector3.Distance(Position, pos);
        public float Distance(GameObject obj) => Distance(obj.transform.position);
        public float Distance(ReferenceHub hub) => Distance(hub.playerMovementSync.RealModelPosition);
        public float Distance(Player player) => Distance(player.Position);
        public float Distance(float x, float y, float z) => Vector3.Distance(Position, new Vector3(x, y, z));

        public void CreatePortal(Player target = null)
        {
            if (Role != RoleType.Scp106)
                return;

            if (target == null)
            {
                _refHub.scp106PlayerScript.CallCmdMakePortal();
            }
            else
            {
                Scp106PlayerScript script = _refHub.scp106PlayerScript;

                Transform transform = target._refHub.transform;

                Debug.DrawRay(transform.position, -transform.up, Color.red, 10f);

                if (script.iAm106 && !script.goingViaThePortal && Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out RaycastHit raycastHit, 10f, script.teleportPlacementMask))
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
                _refHub.scp106PlayerScript.CallCmdMakePortal();
            }
            else
            {
                Scp106PlayerScript script = _refHub.scp106PlayerScript;

                Debug.DrawRay(pos, -Vector3.up, Color.red, 10f);

                if (script.iAm106 && !script.goingViaThePortal && Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out RaycastHit raycastHit, 10f, script.teleportPlacementMask))
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

            _refHub.scp106PlayerScript.CallCmdUsePortal();
        }

        public void Teleport(Room room) => Teleport(MapUtilities.FindSafePosition(room.Position));

        public PlayerEffect GetEffect(EffectType effect) => PlayerUtilities.GetEffect(this, effect);
        public void EnableEffect(EffectType effect, float duration = 0f, bool addIfActive = false) => PlayerUtilities.EnableEffect(this, effect, duration, addIfActive);
        public void DisableEffect(EffectType effect) => PlayerUtilities.DisableEffect(this, effect);
        public void DisableAllEffects() => PlayerUtilities.DisableAllEffects(this);

        public void ChangeEffectIntensity(string type, byte intensity, float duration = 0f) => PlayerUtilities.ChangeEffectIntensity(this, type, intensity, duration);

        public bool GetActiveEffect<T>() where T : PlayerEffect => PlayerUtilities.GetActiveEffect<T>(this);
        public float GetEffectIntensity<T>() where T : PlayerEffect => PlayerUtilities.GetEffectIntensity<T>(this);
        public void ChangeEffectIntensity<T>(byte intensity) where T : PlayerEffect => PlayerUtilities.ChangeEffectIntensity<T>(this, intensity);

        public void EnableEffect<T>(float duration = 0f, bool addIfActive = false) where T : PlayerEffect => _refHub.playerEffectsController.EnableEffect<T>(duration, addIfActive);
        public void DisableEffect<T>() where T : PlayerEffect => _refHub.playerEffectsController.DisableEffect<T>();

        public bool TryGetEffect(EffectType type, out PlayerEffect effect) => PlayerUtilities.TryGetEffect(this, type, out effect);
        public T GetEffect<T>() where T : PlayerEffect => _refHub.playerEffectsController.GetEffect<T>();

        public override string ToString() => $"[{PlayerId}]: {(RemoteAdmin ? "[RA] " : "")}{Nick} [{UserId}] [{IpAddress}] [{Role.AsString()}]";
    }
}