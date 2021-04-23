using Vigilance.API;
using Vigilance.External.Extensions;
using Vigilance.External.Utilities;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.API.Enums;
using UnityEngine;
using Grenades;
using System;
using Scp914;
using System.Collections.Generic;
using PlayableScps;
using Respawning;
using System.Linq;
using CustomPlayerEffects;
using Interactables.Interobjects.DoorUtils;
using LiteNetLib;
using LiteNetLib.Utils;
using Assets._Scripts.Dissonance;

namespace Vigilance.EventSystem.Events
{
    public class AnnounceDecontaminationEvent : Event
    {
        public bool IsGlobal { get; set; }
        public int AnnouncementId { get; set; }
        public bool Allow { get; set; }

        public AnnounceDecontaminationEvent(bool isGlobal, int id, bool allow)
        {
            IsGlobal = isGlobal;
            AnnouncementId = id;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerAnnounceDecontamination)handler).OnAnnounceDecontamination(this);
        }
    }

    public class AnnounceNTFEntranceEvent : Event
    {
        public int SCPsLeft { get; set; }
        public string Unit { get; set; }
        public int Number { get; set; }
        public bool Allow { get; set; }

        public AnnounceNTFEntranceEvent(int scps, string name, int num, bool allow)
        {
            SCPsLeft = scps;
            Unit = name;
            Number = num;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerAnnounceNtfEntrance)handler).OnAnnounceEntrance(this);
        }
    }

    public class AnnounceSCPTerminationEvent : Event
    {
        public Player Killer { get; }
        public Role Role { get; }
        public PlayerStats.HitInfo HitInfo { get; }
        public string Cause { get; }
        public bool Allow { get; set; }

        public AnnounceSCPTerminationEvent(Player killer, Role role, PlayerStats.HitInfo info, string cause, bool allow)
        {
            Killer = killer;
            Role = role;
            HitInfo = info;
            Cause = cause;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerAnnounceScpTermination)handler).OnAnnounceTermination(this);
        }
    }

    public class CancelMedicalItemEvent : Event
    {
        public float Cooldown { get; set; }
        public Player Player { get; }
        public ItemType Item { get; }
        public bool Allow { get; set; }

        public CancelMedicalItemEvent(float cooldown, Player ply, ItemType item, bool allow)
        {
            Cooldown = cooldown;
            Player = ply;
            Item = item;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerCancelMedicalItem)handler).OnCancelMedical(this);
        }
    }

    public class GlobalReportEvent : Event
    {
        public string Reason { get; }
        public Player Reporter { get; }
        public Player Reported { get; }
        public bool Allow { get; set; }

        public GlobalReportEvent(string reason, Player reporter, Player reported, bool allow)
        {
            Reason = reason;
            Reporter = reporter;
            Reported = reported;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGlobalReport)handler).OnGlobalReport(this);
        }
    }

    public class LocalReportEvent : Event
    {
        public string Reason { get; }
        public Player Reporter { get; }
        public Player Reported { get; }
        public bool Allow { get; set; }

        public LocalReportEvent(string reason, Player reporter, Player reported, bool allow)
        {
            Reason = reason;
            Reporter = reporter;
            Reported = reported;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerLocalReport)handler).OnLocalReport(this);
        }
    }

    public class CheckEscapeEvent : Event
    {
        public Player Player { get; }
        public RoleType NewRole { get; set; }
        public bool Allow { get; set; }

        public CheckEscapeEvent(Player player, RoleType newRole, bool allow)
        {
            Player = player;
            NewRole = newRole;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerCheckEscape)handler).OnCheckEscape(this);
        }
    }

    public class CheckRoundEndEvent : Event
    {
        public bool Allow { get; set; }
        public LeadingTeam LeadingTeam { get; set; }

        public CheckRoundEndEvent(bool allow, LeadingTeam leadingTeam)
        {
            Allow = allow;
            LeadingTeam = leadingTeam;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerCheckRoundEnd)handler).OnCheckRoundEnd(this);
        }
    }

    public class ConsoleCommandEvent : Event
    {
        public string Command { get; }
        public string Reply { get; set; }
        public string Color { get; set; }
        public Player Sender { get; }
        public bool Allow { get; set; }

        public ConsoleCommandEvent(string cmd, Player sender, bool allow)
        {
            Command = cmd;
            Sender = sender;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerConsoleCommand)handler).OnConsoleCommand(this);
        }
    }

    public class DecontaminationEvent : Event
    {
        public bool Allow { get; set; }

        public DecontaminationEvent(bool allow) => Allow = allow;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerLczDecontamination)handler).OnDecontamination(this);
        }
    }

    public class DoorInteractEvent : Event
    {
        public bool Allow { get; set; }
        public Player Player { get; }
        public API.Door Door { get; }

        public DoorInteractEvent(bool allow, Player ply, API.Door d)
        {
            Allow = allow;
            Player = ply;
            Door = d;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerDoorInteract)handler).OnDoorInteract(this);
        }
    }

    public class DropItemEvent : Event
    {
        public Inventory.SyncItemInfo Item { get; set; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public DropItemEvent(Inventory.SyncItemInfo itemInfo, Player ply, bool allow)
        {
            Item = itemInfo;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerDropItem)handler).OnDropItem(this);
        }
    }

    public class DroppedItemEvent : Event
    {
        public Pickup Item { get; set; }
        public Player Player { get; }

        public DroppedItemEvent(Pickup item, Player ply)
        {
            Item = item;
            Player = ply;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerDroppedItem)handler).OnItemDropped(this);
        }
    }

    public class ElevatorInteractEvent : Event
    {
        public Lift Lift { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public ElevatorInteractEvent(Lift lift, Player ply, bool allow)
        {
            Lift = lift;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerElevatorInteract)handler).OnElevatorInteract(this);
        }
    }

    public class FemurEnterEvent : Event
    {
        public bool Allow { get; set; }
        public Player Player { get; }

        public FemurEnterEvent(Player ply, bool allow)
        {
            Allow = allow;
            Player = ply;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerFemurEnter)handler).OnFemurEnter(this);
        }
    }

    public class GeneratorInsertEvent : Event
    {
        public Generator Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorInsertEvent(Generator gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGeneratorInsert)handler).OnGeneratorInsert(this);
        }
    }

    public class GeneratorEjectEvent : Event
    {
        public Generator Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorEjectEvent(Generator gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGeneratorEject)handler).OnGeneratorEject(this);
        }
    }

    public class GeneratorUnlockEvent : Event
    {
        public Generator Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorUnlockEvent(Generator gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGeneratorUnlock)handler).OnGeneratorUnlock(this);
        }
    }

    public class GeneratorOpenEvent : Event
    {
        public Generator Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorOpenEvent(Generator gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGeneratorOpen)handler).OnGeneratorOpen(this);
        }
    }

    public class GeneratorCloseEvent : Event
    {
        public Generator Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorCloseEvent(Generator gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGeneratorClose)handler).OnGeneratorClose(this);
        }
    }

    public class GeneratorFinishEvent : Event
    {
        public Generator Generator { get; }
        public bool Allow { get; set; }

        public GeneratorFinishEvent(Generator gen, bool allow)
        {
            Generator = gen;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGeneratorFinish)handler).OnGeneratorFinish(this);
        }
    }

    public class ThrowGrenadeEvent : Event
    {
        public Player Thrower { get; }
        public GrenadeSettings Settings { get; set; }

        public GrenadeType GrenadeType { get; }

        public float ForceMultiplier { get; set; }
        public float Delay { get; set; }

        public int ItemIndex { get; set; }

        public bool Allow { get; set; }

        public ThrowGrenadeEvent(Player ply, GrenadeSettings settings, float forceMultiplier, float delay, int index, bool allow)
        {
            Thrower = ply;
            Settings = settings;
            GrenadeType = (GrenadeType)(int)settings.inventoryID;
            ForceMultiplier = forceMultiplier;
            Delay = delay;
            ItemIndex = index;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerThrowGrenade)handler).OnThrowGrenade(this);
        }
    }

    public class IntercomSpeakEvent : Event
    {
        public Player Speaker { get; }
        public bool Allow { get; set; }

        public IntercomSpeakEvent(Player speak, bool allow)
        {
            Speaker = speak;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerIntercomSpeak)handler).OnSpeak(this);
        }
    }

    public class IntercomCheckSpeakAllowedEvent : Event
    {
        public Player Speaker { get; }
        public DissonanceUserSetup Dissonance { get; }
        public bool Allow { get; set; }

        public IntercomCheckSpeakAllowedEvent(Player speaker, DissonanceUserSetup setup, bool allow)
        {
            Speaker = speaker;
            Dissonance = setup;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerIntercomCheckAllowedSpeak)handler).OnIntercomCheck(this);
        }
    }

    public class AllowChangeItemCheckEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public AllowChangeItemCheckEvent(Player player, bool allow)
        {
            Player = player;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerAllowChangeItemCheck)handler).OnAllowChangeItemCheck(this);
        }
    }

    public class ChangeItemEvent : Event
    {
        public Inventory.SyncItemInfo OldItem { get; }
        public ItemType NewItem { get; set; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public ChangeItemEvent(Inventory.SyncItemInfo oldItem, ItemType newItem, Player ply, bool allow)
        {
            OldItem = oldItem;
            NewItem = newItem;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerChangeItem)handler).OnChangeItem(this);
        }
    }

    public class LockerInteractEvent : Event
    {
        public API.Locker Locker { get; }
        public Chamber Chamber { get; }
        public Player Player { get; }
        public string AccessToken { get; set; }
        public bool Allow { get; set; }

        public LockerInteractEvent(API.Locker locker, Chamber chamber, Player ply, bool allow)
        {
            Locker = locker;
            Chamber = chamber;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerLockerInteract)handler).OnLockerInteract(this);
        }
    }

    public class PickupItemEvent : Event
    {
        public Pickup Item { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public PickupItemEvent(Pickup item, Player ply, bool allow)
        {
            Item = item;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPickupItem)handler).OnPickupItem(this);
        }
    }

    public class PlaceBloodEvent : Event
    {
        public bool Allow { get; set; }
        public Vector3 Position { get; set; }
        public int Type { get; set; }

        public PlaceBloodEvent(bool allow, int type, Vector3 pos)
        {
            Allow = allow;
            Position = pos;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlaceBlood)handler).OnPlaceBlood(this);
        }
    }

    public class PlaceDecalEvent : Event
    {
        public bool Allow { get; set; }
        public Vector3 Position { get; set; }

        public PlaceDecalEvent(bool allow, Vector3 pos)
        {
            Allow = allow;
            Position = pos;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlaceDecal)handler).OnPlaceDecal(this);
        }
    }

    public class BanEvent : Event
    {
        public Player Player { get; }
        public Player Issuer { get; }

        public TimeSpan Expiration { get; set; }
        public TimeSpan Issuance { get; set; }

        public string Reason { get; }
        public bool Allow { get; set; }

        public BanEvent(Player ply, Player issuer, string reason, long issuance, long expiery, bool allow)
        {
            Player = ply;
            Issuer = issuer;
            Reason = reason;
            Expiration = TimeSpan.FromTicks(expiery);
            Issuance = TimeSpan.FromTicks(issuance);
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerBan)handler).OnBan(this);
        }
    }

    public class HandcuffEvent : Event
    {
        public Player Target { get; }
        public Player Cuffer { get; }
        public bool Allow { get; set; }

        public HandcuffEvent(Player ply, Player cuffer, bool allow)
        {
            Target = ply;
            Cuffer = cuffer;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerHandcuff)handler).OnHandcuff(this);
        }
    }

    public class UncuffEvent : Event
    {
        public Player Target { get; }
        public Player Cuffer { get; }
        public bool Allow { get; set; }

        public UncuffEvent(Player ply, Player uncuffer, bool allow)
        {
            Target = ply;
            Cuffer = uncuffer;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerUncuff)handler).OnUncuff(this);
        }
    }

    public class PlayerHurtEvent : Event
    {
        public Player Target { get; }
        public Player Attacker { get; }
        public PlayerStats.HitInfo HitInfo { get; set; }
        public bool Allow { get; set; }

        public DamageTypes.DamageType BaseType => HitInfo.GetDamageType();
        public DamageType DamageType => BaseType.AsDamageType();

        public bool IsPlayer => HitInfo.IsPlayer;
        public float Damage => HitInfo.Amount;

        public int PlayerId => HitInfo.PlayerId;
        public int Time => HitInfo.Time;
        public int Tool => HitInfo.Tool;

        public string AttackerNick => HitInfo.Attacker;
        public string DamageName => HitInfo.GetDamageName();

        public PlayerHurtEvent(Player target, Player attack, PlayerStats.HitInfo hit, bool allow)
        {
            Target = target;
            Attacker = attack;
            HitInfo = hit;
            Allow = allow;
        }

        public void SetDamage(float dmg)
        {
            PlayerStats.HitInfo info = HitInfo;
            info.Amount = dmg;
            HitInfo = info;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerHurt)handler).OnHurt(this);
        }
    }

    public class PlayerInteractEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public PlayerInteractEvent(Player ply, bool allow)
        {
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerInteract)handler).OnInteract(this);
        }
    }

    public class PlayerJoinEvent : Event
    {
        public Player Player { get; }
        public UserIdType AuthType { get; }
        public bool Allow { get; set; }

        public PlayerJoinEvent(Player ply, bool allow)
        {
            Player = ply;
            AuthType = ply.UserIdType;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerJoin)handler).OnJoin(this);
        }
    }

    public class PlayerLeaveEvent : Event
    {
        public Player Player { get; }

        public PlayerLeaveEvent(Player ply)
        {
            Player = ply;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerLeave)handler).OnLeave(this);
        }
    }

    public class PlayerSpawnEvent : Event
    {
        public Player Player { get; }

        public RoleType Role { get; set; }

        public bool Allow { get; set; }

        public PlayerSpawnEvent(Player ply, RoleType role, bool allow)
        {
            Player = ply;
            Role = role;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerSpawn)handler).OnSpawn(this);
        }
    }

    public class WeaponReloadEvent : Event
    {
        public Player Player { get; }
        public Inventory.SyncItemInfo Weapon { get; }
        public bool AnimationOnly { get; set; }
        public bool Allow { get; set; }

        public WeaponReloadEvent(Player ply, bool anim, bool allow)
        {
            Player = ply;
            Weapon = ply.CurrentItem;
            AnimationOnly = anim;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWeaponReload)handler).OnReload(this);
        }
    }

    public class PocketEscapeEvent : Event
    {
        public Player Player { get; }
        public Vector3 Position { get; set; }
        public bool Allow { get; set; }

        public PocketEscapeEvent(Player ply, Vector3 pos, bool allow)
        {
            Player = ply;
            Position = pos;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPocketEscape)handler).OnEscape(this);
        }
    }

    public class PocketEnterEvent : Event
    {
        public Player Player { get; }

        public bool Hurt { get; set; }
        public bool Allow { get; set; }

        public float Damage { get; set; }

        public PocketEnterEvent(Player ply, bool damage, float dmg, bool allow)
        {
            Player = ply;
            Hurt = damage;
            Allow = allow;
            Damage = dmg;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPocketEnter)handler).OnEnter(this);
        }
    }

    public class RemoteAdminCommandEvent : Event
    {
        public Player Issuer { get; }
        public string Command { get; }
        public string Response { get; set; }
        public bool Allow { get; set; }

        public RemoteAdminCommandEvent(Player ply, string cmd, string response, bool allow)
        {
            Issuer = ply;
            Command = cmd;
            Response = response;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerRemoteAdmin)handler).OnRemoteAdminCommand(this);
        }
    }

    public class RoundEndEvent : Event
    {
        public LeadingTeam LeadingTeam { get; }
        public RoundSummary.SumInfo_ClassList StartList { get; set; }
        public RoundSummary.SumInfo_ClassList EndList { get; set; }
        public int TimeToRestart { get; set; }
        public bool Allow { get; set; }

        public RoundEndEvent(LeadingTeam team, RoundSummary.SumInfo_ClassList startList, RoundSummary.SumInfo_ClassList endList, int toRestart, bool allow)
        {
            LeadingTeam = team;
            StartList = startList;
            EndList = endList;
            TimeToRestart = toRestart;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerRoundEnd)handler).OnRoundEnd(this);
        }
    }

    public class RoundStartEvent : Event
    {
        public List<Player> Players => PlayersList.List;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerRoundStart)handler).OnRoundStart(this);
        }
    }

    public class RoundRestartEvent : Event
    {
        public float ReconnectionTime { get; set; }

        public RoundRestartEvent(float time) => ReconnectionTime = time;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerRoundRestart)handler).OnRoundRestart(this);
        }
    }

    public class SCP914UpgradeEvent : Event
    {
        public Scp914Machine Scp914 { get; }
        public IEnumerable<Player> Players { get; }
        public IEnumerable<Pickup> Items { get; }
        public Scp914Knob KnobSetting { get; set; }
        public bool Allow { get; set; }

        public SCP914UpgradeEvent(Scp914Machine machine, IEnumerable<GameObject> playes, IEnumerable<Pickup> items, Scp914Knob knob, bool allow)
        {
            Scp914 = machine;
            Players = playes.Select(h => h.GetPlayer());
            Items = items;
            KnobSetting = knob;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp914Upgrade)handler).OnSCP914Upgrade(this);
        }
    }

    public class SCP096EnrageEvent : Event
    {
        public Scp096 Scp096 { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public SCP096EnrageEvent(Player ply, bool allow)
        {
            Scp096 = ply.CurrentScp as Scp096;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp096Enrage)handler).OnEnrage(this);
        }
    }

    public class SCP096CalmEvent : Event
    {
        public Scp096 Scp096 { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public SCP096CalmEvent(Player ply, bool allow)
        {
            Scp096 = ply.CurrentScp as Scp096;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp096Calm)handler).OnCalm(this);
        }
    }

    public class SCP106ContainEvent : Event
    {
        public Player Killer { get; }
        public Player Scp { get; }
        public bool Allow { get; set; }

        public SCP106ContainEvent(Player killer, Player ply, bool allow)
        {
            Killer = killer;
            Scp = ply;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp106Contain)handler).OnContain(this);
        }
    }

    public class SCP106CreatePortalEvent : Event
    {
        public Player Player { get; }
        public Vector3 Position { get; set; }
        public bool Allow { get; set; }

        public SCP106CreatePortalEvent(Player ply, Vector3 pos, bool allow)
        {
            Player = ply;
            Position = pos;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp106CreatePortal)handler).OnCreatePortal(this);
        }
    }

    public class SCP106TeleportEvent : Event
    {
        public Player Player { get; }
        public Vector3 OldPortal { get; }
        public Vector3 NewPosition { get; set; }
        public bool Allow { get; set; }

        public SCP106TeleportEvent(Player ply, Vector3 old, Vector3 newPos, bool allow)
        {
            Player = ply;
            OldPortal = old;
            NewPosition = newPos;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp106Teleport)handler).OnTeleport(this);
        }
    }

    public class SCP914ActivateEvent : Event
    {
        public Player Player { get; }
        public float Time { get; set; }
        public bool Allow { get; set; }

        public SCP914ActivateEvent(Player ply, float time, bool allow)
        {
            Player = ply;
            Time = time;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp914Activate)handler).OnActivate(this);
        }
    }

    public class SCP914ChangeKnobEvent : Event
    {
        public Player Player { get; }
        public Scp914Machine Scp914 { get; }
        public Scp914Knob OldKnob { get; }
        public Scp914Knob NewKnob { get; set; }
        public bool Allow { get; set; }

        public SCP914ChangeKnobEvent(Player ply, Scp914Machine scp, Scp914Knob old, Scp914Knob newState, bool allow)
        {
            Player = ply;
            Scp914 = scp;
            OldKnob = old;
            NewKnob = newState;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp914ChangeKnob)handler).OnChangeKnob(this);
        }
    }

    public class SetClassEvent : Event
    {
        public Player Player { get; }
        public RoleType Role { get; set; }
        public bool Allow { get; set; }

        public SetClassEvent(Player ply, RoleType role, bool allow)
        {
            Player = ply;
            Role = role;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerSetClass)handler).OnSetClass(this);
        }
    }

    public class SetGroupEvent : Event
    {
        public Player Player { get; }
        public UserGroup Group { get; set; }
        public bool Allow { get; set; }

        public SetGroupEvent(Player ply, UserGroup group, bool allow)
        {
            Player = ply;
            Group = group;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerSetGroup)handler).OnSetGroup(this);
        }
    }

    public class WeaponShootEvent : Event
    {
        public Player Player { get; }
        public GameObject Target { get; }

        public Inventory.SyncItemInfo Item { get; }
        public WeaponManager.Weapon Weapon { get; }

        public Vector3 Direction { get; }
        public Vector3 SourcePos { get; }
        public Vector3 TargetPos { get; }

        public HitBoxType HitBoxType { get; }

        public int WeaponIndex { get; }
        public int ItemIndex { get; }

        public bool Allow { get; set; }

        public WeaponShootEvent(Player ply, GameObject go, Inventory.SyncItemInfo item, WeaponManager.Weapon wep, Vector3 dir, Vector3 sourcePos, Vector3 targetPos, HitBoxType hitBoxType,
            int wepIndex, int itemIndex, bool allow)
        {
            Player = ply;
            Target = go;
            Item = item;
            Weapon = wep;
            Direction = dir;
            SourcePos = sourcePos;
            TargetPos = targetPos;
            HitBoxType = hitBoxType;
            WeaponIndex = wepIndex;
            ItemIndex = itemIndex;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWeaponShoot)handler).OnShoot(this);
        }
    }

    public class WeaponLateShootEvent : Event
    {
        public Player Player { get; }
        public Player Target { get; }
        public float Damage { get; set; }
        public bool Allow { get; set; }

        public WeaponLateShootEvent(Player attacker, Player target, float damage, bool allow)
        {
            Player = attacker;
            Target = target;
            Damage = damage;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWeaponLateShoot)handler).OnLateShoot(this);
        }
    }

    public class SpawnRagdollEvent : Event
    {
        public Role Role { get; set; }
        public GameObject RagdollModel { get; set; }
        public Offset Offset { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Quaternion Rotation { get; set; }
        public RoleType RoleId { get; set; }
        public PlayerStats.HitInfo HitInfo { get; set; }
        public Ragdoll.Info RagdollInfo { get; set; }
        public bool AllowRecall { get; set; }
        public string OwnerId { get; set; }
        public string OwnerNick { get; set; }
        public int PlayerId { get; set; }
        public bool Allow { get; set; }

        public SpawnRagdollEvent(Role role, GameObject modelRagdoll, Offset offset, Vector3 pos, Vector3 vel, Quaternion rot, 
            RoleType roleId, PlayerStats.HitInfo hitInfo, bool allowRecall, 
            string ownerId, string ownerNick, int playerId, Ragdoll.Info info, bool allow)
        {
            Role = role;
            RagdollModel = modelRagdoll;
            Offset = offset;
            Position = pos;
            Velocity = vel;
            Rotation = rot;
            RoleId = roleId;
            HitInfo = hitInfo;
            RagdollInfo = info;
            AllowRecall = allowRecall;
            OwnerId = ownerId;
            OwnerNick = ownerNick;
            PlayerId = playerId;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerSpawnRagdoll)handler).OnSpawnRagdoll(this);
        }
    }

    public class SyncDataEvent : Event
    {
        public Player Player { get; }
        public Vector2 Speed { get; set; }
        public byte Animation { get; set; }
        public bool Allow { get; set; }

        public SyncDataEvent(Player ply, Vector2 speed, byte curAnim, bool allow)
        {
            Player = ply;
            Speed = speed;
            Animation = curAnim;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerSyncData)handler).OnSyncData(this);
        }
    }

    public class TeamRespawnEvent : Event
    {
        public List<Player> Players { get; set; }
        public SpawnableTeamType Team { get; set; }
        public bool Allow { get; set; }

        public TeamRespawnEvent(List<Player> players, SpawnableTeamType team, bool allow)
        {
            Players = players;
            Team = team;
            Allow = allow;
        }

        public void Despawn()
        {
            foreach (Player player in Players)
                player.SetRole(RoleType.Spectator);
        }

        public void ClearPlayers() => Players.Clear();

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerTeamRespawn)handler).OnTeamRespawn(this);
        }
    }

    public class TriggerTeslaEvent : Event
    {
        public Player Player { get; }
        public Tesla Tesla { get; }
        public bool Allow { get; set; }

        public TriggerTeslaEvent(Player ply, Tesla tesla, bool allow)
        {
            Player = ply;
            Tesla = tesla;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerTriggerTesla)handler).OnTriggerTesla(this);
        }
    }

    public class UseItemEvent : Event
    {
        public Player Player { get; }
        public ConsumableAndWearableItems.UsableItem Item { get; set; }
        public bool Allow { get; set; }

        public UseItemEvent(Player ply, ConsumableAndWearableItems.UsableItem item, bool allow)
        {
            Player = ply;
            Item = item;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerUseItem)handler).OnUseItem(this);
        }
    }

    public class WaitingForPlayersEvent : Event
    {
        public bool LobbyLock { get => Round.LobbyLock; set => Round.LobbyLock = value; }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWaitingForPlayers)handler).OnWaitingForPlayers(this);
        }
    }

    public class DetonationEvent : Event
    {
        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWarheadDetonate)handler).OnDetonate(this);
        }
    }

    public class WarheadCancelEvent : Event
    {
        public Player Player { get; }
        public float TimeLeft { get; set; }
        public bool Allow { get; set; }

        public WarheadCancelEvent(Player ply, float time, bool allow)
        {
            Player = ply;
            TimeLeft = time;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWarheadCancel)handler).OnCancel(this);
        }
    }

    public class WarheadStartEvent : Event
    {
        public Player Player { get; }
        public float TimeLeft { get; set; }
        public bool Allow { get; set; }

        public WarheadStartEvent(Player ply, float time, bool allow)
        {
            Player = ply;
            TimeLeft = time;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWarheadStart)handler).OnStart(this);
        }
    }

    public class WarheadKeycardAccessEvent : Event
    {
        public Player Player { get; }
        public Inventory.SyncItemInfo Keycard { get; }
        public bool Allow { get; set; }

        public WarheadKeycardAccessEvent(Player ply, Inventory.SyncItemInfo card, bool allow)
        {
            Player = ply;
            Keycard = card;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWarheadKeycardAccess)handler).OnAccess(this);
        }
    }

    public class SCP049RecallEvent : Event
    {
        public Player Player { get; }
        public Player Target { get; }
        public Ragdoll Ragdoll { get; }
        public bool Allow { get; set; }

        public SCP049RecallEvent(Player ply, Ragdoll ragdoll, bool allow)
        {
            Player = ply;
            Target = ragdoll.GetOwner();
            Ragdoll = ragdoll;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp049Recall)handler).OnRecall(this);
        }
    }

    public class SCP079GainExpEvent : Event
    {
        public Player Player { get; }
        public ExpGainType ExpGainType { get; }
        public float Experience { get; set; }
        public bool Allow { get; set; }

        public SCP079GainExpEvent(Player ply, ExpGainType gainType, float exp, bool allow)
        {
            Player = ply;
            ExpGainType = gainType;
            Experience = exp;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp079GainExp)handler).OnGainExp(this);
        }
    }

    public class SCP079GainLvlEvent : Event
    {
        public Player Player { get; }
        public int Level { get; set; }
        public bool Allow { get; set; }

        public SCP079GainLvlEvent(Player ply, int level, bool allow)
        {
            Player = ply;
            Level = level;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp079GainLvl)handler).OnGainLvl(this);
        }
    }

    public class SCP079InteractEvent : Event
    {
        public Player Player { get; }
        public API.Enums.Scp079Interaction Interaction { get; set; }
        public GameObject Target { get; }
        public float ExpCost { get; set; }
        public bool Allow { get; set; }

        public SCP079InteractEvent(Player ply, Scp079Interactable.InteractableType inter, GameObject target, float expCost, bool allow)
        {
            Player = ply;
            Interaction = (API.Enums.Scp079Interaction)(int)inter;
            Target = target;
            ExpCost = expCost;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp079Interact)handler).OnInteract(this);
        }
    }

    public class ServerCommandEvent : Event
    {
        public ServerConsoleSender Sender { get; }
        public string Command { get; }
        public string Response { get; set; }
        public bool Allow { get; set; }

        public ServerCommandEvent(string cmd, string res, bool all)
        {
            Sender = ServerConsole._scs;
            Command = cmd;
            Response = res;
            Allow = all;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerServerCommand)handler).OnServerCommand(this);
        }
    }

    public class RoundShowSummaryEvent : Event
    {
        public RoundSummary.SumInfo_ClassList ClassListStart { get; set; }
        public RoundSummary.SumInfo_ClassList ClassListEnd { get; set; }
        public RoundSummary.LeadingTeam Team { get; set; }
        public bool Allow { get; set; }

        public RoundShowSummaryEvent(RoundSummary.SumInfo_ClassList start, RoundSummary.SumInfo_ClassList end, RoundSummary.LeadingTeam team, bool allow)
        {
            ClassListStart = start;
            ClassListEnd = end;
            Team = team;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerRoundShowSummary)handler).OnShowSummary(this);
        }
    }

    public class ConsoleAddLogEvent : Event
    {
        public string Text { get; }
        public bool Allow { get; set; }

        public ConsoleAddLogEvent(string msg, bool allow)
        {
            Text = msg;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerConsoleAddLog)handler).OnAddLog(this);
        }
    }

    public class PlayerDieEvent : Event
    {
        public Player Target { get; }
        public Player Attacker { get; }
        public PlayerStats.HitInfo HitInfo { get; set; }
        public bool Allow { get; set; }
        public bool SpawnRagdoll { get; set; }

        public DamageTypes.DamageType BaseType => HitInfo.GetDamageType();
        public DamageType DamageType => BaseType.AsDamageType();

        public bool IsPlayer => HitInfo.IsPlayer;
        public float Damage => HitInfo.Amount;

        public int PlayerId => HitInfo.PlayerId;
        public int Time => HitInfo.Time;
        public int Tool => HitInfo.Tool;

        public string AttackerNick => HitInfo.Attacker;
        public string DamageName => HitInfo.GetDamageName();

        public PlayerDieEvent(Player target, Player attacker, PlayerStats.HitInfo info, bool allow)
        {
            Target = target;
            Attacker = attacker;
            HitInfo = info;
            Allow = allow;
            SpawnRagdoll = PluginManager.Config.SpawnRagdolls;
        }

        public void SetDamage(float dmg)
        {
            PlayerStats.HitInfo info = HitInfo;
            info.Amount = dmg;
            HitInfo = info;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerDie)handler).OnPlayerDie(this);
        }
    }

    public class Scp096AddTargetEvent : Event
    {
        public Scp096 Scp { get; }
        public Player Player { get; }
        public Player Target { get; }
        public bool Allow { get; set; }

        public Scp096AddTargetEvent(Player scp, Player target, bool allow)
        {
            Scp = scp.CurrentScp as Scp096;
            Player = scp;
            Target = target;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp096AddTarget)handler).OnScp096AddTarget(this);
        }
    }

    public class Scp914UpgradeItemEvent : Event
    {
        public ItemType Input { get; }
        public ItemType Output { get; set; }
        public bool Allow { get; set; }

        public Scp914UpgradeItemEvent(ItemType input, bool allow)
        {
            Input = input;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp914UpgradeItem)handler).OnScp914UpgradeItem(this);
        }
    }

    public class Scp914UpgradePickupEvent : Event
    {
        public Pickup Input { get; }
        public ItemType Output { get; set; }

        public bool Allow { get; set; }

        public Scp914UpgradePickupEvent(Pickup input, ItemType output, bool allow)
        {
            Input = input;
            Output = output;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp914UpgradePickup)handler).OnUpgradePickup(this);
        }
    }

    public class Scp914UpgradePlayerEvent : Event
    {
        public Player Player { get; set; }
        public bool Allow { get; set; }

        public Scp914UpgradePlayerEvent(Player player, bool allow)
        {
            Player = player;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp914UpgradePlayer)handler).OnScp914UpgradePlayer(this);
        }
    }

    public class PlayerUseLockerEvent : Event
    {
        public Player Player { get; }
        public Locker Locker { get; }
        public string AccessToken { get; set; }
        public bool Allow { get; set; }

        public PlayerUseLockerEvent(Player player, Locker locker, string token, bool allow)
        {
            Player = player;
            Locker = locker;
            AccessToken = token;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerUseLocker)handler).OnUseLocker(this);
        }
    }

    public class WarheadLeverSwitchEvent : Event
    {
        public Player Player { get; }
        public bool CurrentState { get; }
        public bool NewState { get; set; }
        public bool Allow { get; set; }

        public WarheadLeverSwitchEvent(Player p, bool s, bool c, bool a)
        {
            Player = p;
            CurrentState = c;
            NewState = s;
            Allow = a;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerWarheadLeverSwitch)handler).OnLeverSwitch(this);
        }
    }

    public class GenerateSeedEvent : Event
    {
        public int Seed { get; set; }

        public GenerateSeedEvent(int seed) => Seed = seed;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGenerateSeed)handler).OnGenerateSeed(this);
        }
    }

    public class ChangeItemDurabilityEvent : Event
    {
        public Player Player { get; }
        public Inventory.SyncItemInfo Item { get; }
        public float Durability { get; set; }
        public bool Allow { get; set; }

        public ChangeItemDurabilityEvent(Player player, Inventory.SyncItemInfo item, float durability, bool allow)
        {
            Player = player;
            Item = item;
            Durability = durability;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerItemChangeDurability)handler).OnChangeDurability(this);
        }
    }

    public class ChangeItemAttachmentsEvent : Event
    {
        public Player Player { get; }
        public Inventory.SyncItemInfo Item { get; }
        public SightType OldSight { get; }
        public SightType NewSight { get; set; }

        public BarrelType OldBarell { get; }
        public BarrelType NewBarrel { get; set; }

        public OtherType OldOther { get; }
        public OtherType NewOther { get; set; }

        public bool Allow { get; set; }

        public ChangeItemAttachmentsEvent(Player player, Inventory.SyncItemInfo item, SightType oldS, SightType newS, BarrelType oldB, BarrelType newB, OtherType oldO, OtherType newO, bool allow)
        {
            Player = player;
            Item = item;
            OldSight = oldS;
            NewSight = newS;
            OldBarell = oldB;
            NewBarrel = newB;
            OldOther = oldO;
            NewOther = newO;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerItemChangeAttachments)handler).OnChangeAttachments(this);
        }
    }

    public class DamageWindowEvent : Event
    {
        public Player Player { get; }
        public DamageType DamageType { get; }
        public Window Window { get; }
        public float Damage { get; set; }
        public bool Allow { get; set; }

        public DamageWindowEvent(Player player, Window window, DamageType type, float damage, bool allow)
        {
            Player = player;
            DamageType = type;
            Window = window;
            Damage = damage;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerDamageWindow)handler).OnDamageWindow(this);
        }
    }

    public class GrenadeExplodeEvent : Event
    {
        public Player Player { get; }
        public Grenade Grenade { get; }
        public GrenadeType GrenadeType { get; }
        public bool Allow { get; set; }

        public GrenadeExplodeEvent(Player player, Grenade grenade, GrenadeType type, bool allow)
        {
            Player = player;
            Grenade = grenade;
            GrenadeType = type;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerGrenadeExplode)handler).OnGrenadeExplode(this);
        }
    }

    public class MapGeneratedEvent : Event
    {
        public int Seed { get; }

        public MapGeneratedEvent(int seed) => Seed = seed;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerMapGenerated)handler).OnMapGenerated(this);
        }
    }

    public class PreAuthEvent : Event
    {
        public string UserId { get; }
        public int ReaderPosition { get; }
        public byte Flags { get; }
        public string Country { get; }
        public ConnectionRequest Request { get; }
        public bool Allow { get; private set; }

        public PreAuthEvent(string id, int reader, byte flags, string country, ConnectionRequest request, bool allow = true)
        {
            UserId = id;
            ReaderPosition = reader;
            Flags = flags;
            Country = country;
            Request = request;
            Allow = allow;
        }

        public void Redirect(ushort port, bool isForced) => Reject(RejectionReason.Redirect, isForced, null, 0, 0, port);
        public void Reject(string rejectionReason, bool isForced) => Reject(RejectionReason.Custom, isForced, rejectionReason);
        public void RejectBanned(string banReason, long expiration, bool isForced) => Reject(RejectionReason.Banned, isForced, banReason, expiration);
        public void RejectBanned(string banReason, DateTime expiration, bool isForced) => Reject(RejectionReason.Banned, isForced, banReason, expiration.Ticks);

        public void Delay(byte seconds, bool isForced)
        {
            if (seconds < 1 && seconds > 25)
                throw new Exception("Delay duration must be between 1 and 25 seconds.");
            Reject(RejectionReason.Delay, isForced, null, 0, seconds);
        }

        public void Reject(NetDataWriter writer, bool isForced)
        {
            if (!Allow)
                return;
            Allow = false;
            if (isForced)
                Request.RejectForce(writer);
            else
                Request.Reject(writer);
        }

        public void Reject(RejectionReason rejectionReason, bool isForced, string customReason = null, long expiration = 0, byte seconds = 0, ushort port = 0)
        {
            if (customReason != null && customReason.Length > 400)
                throw new ArgumentOutOfRangeException(nameof(rejectionReason), "Reason can't be longer than 400 characters.");
            if (!Allow)
                return;
            Allow = false;
            NetDataWriter rejectData = new NetDataWriter();

            switch (rejectionReason)
            {
                case RejectionReason.Banned:
                    rejectData.Put(expiration);
                    rejectData.Put(customReason);
                    break;
                case RejectionReason.Custom:
                    rejectData.Put(customReason);
                    break;
                case RejectionReason.Delay:
                    rejectData.Put(seconds);
                    break;
                case RejectionReason.Redirect:
                    rejectData.Put(port);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rejectionReason), rejectionReason, null);
            }

            if (isForced)
                Request.RejectForce(rejectData);
            else
                Request.Reject(rejectData);
        }

        public void Disallow() => Allow = false;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPreAuth)handler).OnPreAuth(this);
        }
    }

    public class KickEvent : Event
    {
        public Player Issuer { get; }
        public Player Kicked { get; }
        public string Reason { get; set; }
        public bool Allow { get; set; }

        public KickEvent(Player issuer, Player kicked, string reason, bool allow)
        {
            Issuer = issuer;
            Kicked = kicked;
            Reason = reason;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerKick)handler).OnKick(this);
        }
    }

    public class DequipMedicalItemEvent : Event
    {
        public Player Player { get; }
        public ConsumableAndWearableItems.UsableItem Item { get; }
        public bool Allow { get; set; }

        public DequipMedicalItemEvent(Player player, ConsumableAndWearableItems.UsableItem item, bool allow)
        {
            Player = player;
            Item = item;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerMedicalDequip)handler).OnDequipMedical(this);
        }
    }

    public class PlayerVerifiedEvent : Event
    {
        public Player Player { get; }

        public PlayerVerifiedEvent(Player player) => Player = player;

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerVerified)handler).OnVerified(this);
        }
    }

    public class PlayerReceiveEffect : Event
    {
        public Player Player { get; }
        public PlayerEffect Effect { get; }
        public EffectType Type { get; }
        public byte Intensity { get; set; }
        public bool Allow { get; set; }

        public PlayerReceiveEffect(Player player, PlayerEffect effect, EffectType type, byte intensity, bool allow)
        {
            Player = player;
            Effect = effect;
            Type = type;
            Intensity = intensity;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPlayerReceiveEffect)handler).OnReceiveEffect(this);
        }
    }

    public class WorkStationActivateEvent : Event
    {
        public Player Player { get; }
        public Workstation Workstation { get; }
        public bool Allow { get; set; }

        public WorkStationActivateEvent(Player player, Workstation station, bool allow)
        {
            Player = player;
            Workstation = station;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerActivateWorkStation)handler).OnActivateWorkStation(this);
        }
    }

    public class WorkStationDeactivateEvent : Event
    {
        public Player Player { get; }
        public Workstation Workstation { get; }
        public bool Allow { get; set; }

        public WorkStationDeactivateEvent(Player player, Workstation station, bool allow)
        {
            Player = player;
            Workstation = station;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerDeactivateWorkStation)handler).OnDeactivateWorkStation(this);
        }
    }

    public class ChangeMuteStatusEvent : Event
    {
        public Player Player { get; }
        public bool Old { get; }
        public bool New { get; set; }
        public bool Allow { get; set; }

        public ChangeMuteStatusEvent(Player player, bool old, bool n, bool allow)
        {
            Player = player;
            Old = old;
            New = n;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerChangeMuteStatus)handler).OnChangeMuteStatus(this);
        }
    }

    public class Scp079SwitchCameraEvent : Event
    {
        public Player Player { get; }
        public API.Enums.CameraType Old { get; }
        public API.Enums.CameraType New { get; set; }
        public bool Allow { get; set; }

        public Scp079SwitchCameraEvent(Player player, API.Enums.CameraType old, API.Enums.CameraType n, bool allow)
        {
            Player = player;
            Old = old;
            New = n;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp079SwitchCamera)handler).OnSwitchCamera(this);
        }
    }

    public class Scp079RecontainEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public Scp079RecontainEvent(Player player, bool allow)
        {
            Player = player;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp079Recontain)handler).OnRecontain(this);
        }
    }

    public class Scp096PryGateEvent : Event
    {
        public Scp096 Scp { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public Scp096PryGateEvent(Player player, bool allow)
        {
            Scp = player.CurrentScp as Scp096;
            Player = player;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp096PryGate)handler).OnPryGate(this);
        }
    }

    public class PocketDieEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public PocketDieEvent(Player player, bool allow)
        {
            Player = player;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerPocketDie)handler).OnPocketDie(this);
        }
    }

    public class LiftChangeStatusEvent : Event
    {
        public Elevator Elevator { get; }
        public Lift.Status Previous { get; }
        public Lift.Status New { get; }

        public LiftChangeStatusEvent(Elevator elevator, Lift.Status prev, Lift.Status next)
        {
            Elevator = elevator;
            Previous = prev;
            New = next;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerLiftChangeStatus)handler).OnLiftChangeStatus(this);
        }
    }

    public class LiftTeleportEvent : Event
    {
        public Elevator Elevator { get; }
        public Dictionary<Player, Vector3> PlayerPositions { get; set; }
        public Dictionary<Scp106PlayerScript, Vector3> PortalPositions { get; set; }
        public Dictionary<GameObject, Vector3> PickupPositions { get; set; }
        public Dictionary<GameObject, Vector3> PickupAngles { get; set; }
        public Dictionary<Ragdoll, Vector3> RagdollPositions { get; set; }
        public Dictionary<Ragdoll, Vector3> RagdollAngles { get; set; }
        public bool Allow { get; set; }


        public LiftTeleportEvent(Elevator elevator, 
            Dictionary<Player, Vector3> plys, 

            Dictionary<Scp106PlayerScript, Vector3> portals, 

            Dictionary<GameObject, Vector3> pickupPos,
            Dictionary<GameObject, Vector3> pickupAngles,
            
            Dictionary<Ragdoll, Vector3> ragdollPos,
            Dictionary<Ragdoll, Vector3> ragdollAngles)
        {
            Elevator = elevator;

            PlayerPositions = plys;

            PortalPositions = portals;

            PickupPositions = pickupPos;
            PickupAngles = pickupAngles;

            RagdollPositions = ragdollPos;
            RagdollAngles = ragdollAngles;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerLiftTeleport)handler).OnLiftTeleport(this);
        }
    }

    public class Scp049InteractEvent : Event
    {
        public Player Player { get; }
        public Scp049 Scp { get; }
        public Scp049InteractionType Type { get; }
        public GameObject Target { get; }
        public bool Allow { get; set; }

        public Scp049InteractEvent(Player player, Scp049 scp, Scp049InteractionType type, GameObject target, bool allow)
        {
            Player = player;
            Scp = scp;
            Type = type;
            Target = target;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerScp049Interact)handler).OnScp049Interact(this);
        }
    }

    public class SpawnDoorEvent : Event
    {
        public DoorVariant Door { get; set; }
        public DoorVariant Prefab { get; }
        public bool Allow { get; set; }

        public SpawnDoorEvent(DoorVariant door, DoorVariant prefab, bool allow)
        {
            Door = door;
            Prefab = prefab;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerSpawnDoor)handler).OnDoorSpawn(this);
        }
    }

    public class SpawnItemEvent : Event
    {
        public Pickup Pickup { get; }
        public ItemType ItemId { get; set; }
        public float Durability { get; set; }
        public Player Owner { get; set; }
        public Pickup.WeaponModifiers WeaponModifiers { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public bool Allow { get; set; }

        public SpawnItemEvent(Pickup pickup, ItemType id, float dur, Player ownr, Pickup.WeaponModifiers mods, Vector3 pos, Quaternion rot, bool allow)
        {
            Pickup = pickup;
            ItemId = id;
            Durability = dur;
            Owner = ownr;
            WeaponModifiers = mods;
            Position = pos;
            Rotation = rot;
            Allow = allow;
        }

        public override void Execute(IEventHandler handler)
        {
            ((IHandlerSpawnItem)handler).OnSpawnItem(this);
        }
    }
}