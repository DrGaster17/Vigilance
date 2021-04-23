using Vigilance.EventSystem.Events;

namespace Vigilance.EventSystem.EventHandlers
{
    public interface IHandlerAnnounceDecontamination : IEventHandler
    {
        void OnAnnounceDecontamination(AnnounceDecontaminationEvent ev);
    }

    public interface IHandlerAnnounceNtfEntrance : IEventHandler
    {
        void OnAnnounceEntrance(AnnounceNTFEntranceEvent ev);
    }

    public interface IHandlerAnnounceScpTermination : IEventHandler
    {
        void OnAnnounceTermination(AnnounceSCPTerminationEvent ev);
    }

    public interface IHandlerCancelMedicalItem : IEventHandler
    {
        void OnCancelMedical(CancelMedicalItemEvent ev);
    }

    public interface IHandlerGlobalReport : IEventHandler
    {
        void OnGlobalReport(GlobalReportEvent ev);
    }

    public interface IHandlerLocalReport : IEventHandler
    {
        void OnLocalReport(LocalReportEvent ev);
    }

    public interface IHandlerCheckEscape : IEventHandler
    {
        void OnCheckEscape(CheckEscapeEvent ev);
    }

    public interface IHandlerCheckRoundEnd : IEventHandler
    {
        void OnCheckRoundEnd(CheckRoundEndEvent ev);
    }

    public interface IHandlerConsoleCommand : IEventHandler
    {
        void OnConsoleCommand(ConsoleCommandEvent ev);
    }

    public interface IHandlerLczDecontamination : IEventHandler
    {
        void OnDecontamination(DecontaminationEvent ev);
    }

    public interface IHandlerDoorInteract : IEventHandler
    {
        void OnDoorInteract(DoorInteractEvent ev);
    }

    public interface IHandlerDropItem : IEventHandler
    {
        void OnDropItem(DropItemEvent ev);
    }

    public interface IHandlerElevatorInteract : IEventHandler
    {
        void OnElevatorInteract(ElevatorInteractEvent ev);
    }

    public interface IHandlerFemurEnter : IEventHandler
    {
        void OnFemurEnter(FemurEnterEvent ev);
    }

    public interface IHandlerGeneratorInsert : IEventHandler
    {
        void OnGeneratorInsert(GeneratorInsertEvent ev);
    }

    public interface IHandlerGeneratorEject : IEventHandler
    {
        void OnGeneratorEject(GeneratorEjectEvent ev);
    }

    public interface IHandlerGeneratorUnlock : IEventHandler
    {
        void OnGeneratorUnlock(GeneratorUnlockEvent ev);
    }

    public interface IHandlerGeneratorOpen : IEventHandler
    {
        void OnGeneratorOpen(GeneratorOpenEvent ev);
    }

    public interface IHandlerGeneratorClose : IEventHandler
    {
        void OnGeneratorClose(GeneratorCloseEvent ev);
    }

    public interface IHandlerGeneratorFinish : IEventHandler
    {
        void OnGeneratorFinish(GeneratorFinishEvent ev);
    }

    public interface IHandlerThrowGrenade : IEventHandler
    {
        void OnThrowGrenade(ThrowGrenadeEvent ev);
    }

    public interface IHandlerIntercomSpeak : IEventHandler
    {
        void OnSpeak(IntercomSpeakEvent ev);
    }

    public interface IHandlerChangeItem : IEventHandler
    {
        void OnChangeItem(ChangeItemEvent ev);
    }

    public interface IHandlerLockerInteract : IEventHandler
    {
        void OnLockerInteract(LockerInteractEvent ev);
    }

    public interface IHandlerPickupItem : IEventHandler
    {
        void OnPickupItem(PickupItemEvent ev);
    }

    public interface IHandlerPlaceBlood : IEventHandler
    {
        void OnPlaceBlood(PlaceBloodEvent ev);
    }

    public interface IHandlerPlaceDecal : IEventHandler
    {
        void OnPlaceDecal(PlaceDecalEvent ev);
    }

    public interface IHandlerBan : IEventHandler
    {
        void OnBan(BanEvent ev);
    }

    public interface IHandlerHandcuff : IEventHandler
    {
        void OnHandcuff(HandcuffEvent ev);
    }

    public interface IHandlerUncuff : IEventHandler
    {
        void OnUncuff(UncuffEvent ev);
    }

    public interface IHandlerPlayerHurt : IEventHandler
    {
        void OnHurt(PlayerHurtEvent ev);
    }

    public interface IHandlerPlayerInteract : IEventHandler
    {
        void OnInteract(PlayerInteractEvent ev);
    }

    public interface IHandlerPlayerJoin : IEventHandler
    {
        void OnJoin(PlayerJoinEvent ev);
    }

    public interface IHandlerPlayerLeave : IEventHandler
    {
        void OnLeave(PlayerLeaveEvent ev);
    }

    public interface IHandlerWeaponReload : IEventHandler
    {
        void OnReload(WeaponReloadEvent ev);
    }

    public interface IHandlerPlayerSpawn : IEventHandler
    {
        void OnSpawn(PlayerSpawnEvent ev);
    }

    public interface IHandlerPocketEscape : IEventHandler
    {
        void OnEscape(PocketEscapeEvent ev);
    }

    public interface IHandlerPocketEnter : IEventHandler
    {
        void OnEnter(PocketEnterEvent ev);
    }

    public interface IHandlerRemoteAdmin : IEventHandler
    {
        void OnRemoteAdminCommand(RemoteAdminCommandEvent ev);
    }

    public interface IHandlerRoundEnd : IEventHandler
    {
        void OnRoundEnd(RoundEndEvent ev);
    }

    public interface IHandlerRoundStart : IEventHandler
    {
        void OnRoundStart(RoundStartEvent ev);
    }

    public interface IHandlerRoundRestart : IEventHandler
    {
        void OnRoundRestart(RoundRestartEvent ev);
    }

    public interface IHandlerScp914Upgrade : IEventHandler
    {
        void OnSCP914Upgrade(SCP914UpgradeEvent ev);
    }

    public interface IHandlerScp096Enrage : IEventHandler
    {
        void OnEnrage(SCP096EnrageEvent ev);
    }

    public interface IHandlerScp096Calm : IEventHandler
    {
        void OnCalm(SCP096CalmEvent ev);
    }

    public interface IHandlerScp106Contain : IEventHandler
    {
        void OnContain(SCP106ContainEvent ev);
    }

    public interface IHandlerScp106CreatePortal : IEventHandler
    {
        void OnCreatePortal(SCP106CreatePortalEvent ev);
    }

    public interface IHandlerScp106Teleport : IEventHandler
    {
        void OnTeleport(SCP106TeleportEvent ev);
    }

    public interface IHandlerScp914Activate : IEventHandler
    {
        void OnActivate(SCP914ActivateEvent ev);
    }

    public interface IHandlerScp914ChangeKnob : IEventHandler
    {
        void OnChangeKnob(SCP914ChangeKnobEvent ev);
    }

    public interface IHandlerSetClass : IEventHandler
    {
        void OnSetClass(SetClassEvent ev);
    }

    public interface IHandlerSetGroup : IEventHandler
    {
        void OnSetGroup(SetGroupEvent ev);
    }

    public interface IHandlerWeaponShoot : IEventHandler
    {
        void OnShoot(WeaponShootEvent ev);
    }

    public interface IHandlerWeaponLateShoot : IEventHandler
    {
        void OnLateShoot(WeaponLateShootEvent ev);
    }

    public interface IHandlerSpawnRagdoll : IEventHandler
    {
        void OnSpawnRagdoll(SpawnRagdollEvent ev);
    }

    public interface IHandlerSyncData : IEventHandler
    {
        void OnSyncData(SyncDataEvent ev);
    }

    public interface IHandlerTeamRespawn : IEventHandler
    {
        void OnTeamRespawn(TeamRespawnEvent ev);
    }

    public interface IHandlerTriggerTesla : IEventHandler
    {
        void OnTriggerTesla(TriggerTeslaEvent ev);
    }

    public interface IHandlerUseItem : IEventHandler
    {
        void OnUseItem(UseItemEvent ev);
    }

    public interface IHandlerWaitingForPlayers : IEventHandler
    {
        void OnWaitingForPlayers(WaitingForPlayersEvent ev);
    }

    public interface IHandlerWarheadDetonate : IEventHandler
    {
        void OnDetonate(DetonationEvent ev);
    }

    public interface IHandlerWarheadCancel : IEventHandler
    {
        void OnCancel(WarheadCancelEvent ev);
    }

    public interface IHandlerWarheadStart : IEventHandler
    {
        void OnStart(WarheadStartEvent ev);
    }

    public interface IHandlerWarheadKeycardAccess : IEventHandler
    {
        void OnAccess(WarheadKeycardAccessEvent ev);
    }

    public interface IHandlerWarheadLeverSwitch : IEventHandler
    {
        void OnLeverSwitch(WarheadLeverSwitchEvent ev);
    }

    public interface IHandlerScp049Recall : IEventHandler
    {
        void OnRecall(SCP049RecallEvent ev);
    }

    public interface IHandlerScp079GainExp : IEventHandler
    {
        void OnGainExp(SCP079GainExpEvent ev);
    }

    public interface IHandlerScp079GainLvl : IEventHandler
    {
        void OnGainLvl(SCP079GainLvlEvent ev);
    }

    public interface IHandlerScp079Interact : IEventHandler
    {
        void OnInteract(SCP079InteractEvent ev);
    }

    public interface IHandlerServerCommand : IEventHandler
    {
        void OnServerCommand(ServerCommandEvent ev);
    }

    public interface IHandlerRoundShowSummary : IEventHandler
    {
        void OnShowSummary(RoundShowSummaryEvent ev);
    }

    public interface IHandlerConsoleAddLog : IEventHandler
    {
        void OnAddLog(ConsoleAddLogEvent ev);
    }

    public interface IHandlerPlayerDie : IEventHandler
    {
        void OnPlayerDie(PlayerDieEvent ev);
    }

    public interface IHandlerDroppedItem : IEventHandler
    {
        void OnItemDropped(DroppedItemEvent ev);
    }

    public interface IHandlerScp096AddTarget : IEventHandler
    {
        void OnScp096AddTarget(Scp096AddTargetEvent ev);
    }

    public interface IHandlerScp914UpgradeItem : IEventHandler
    {
        void OnScp914UpgradeItem(Scp914UpgradeItemEvent ev);
    }

    public interface IHandlerScp914UpgradePlayer : IEventHandler
    {
        void OnScp914UpgradePlayer(Scp914UpgradePlayerEvent ev);
    }

    public interface IHandlerPlayerUseLocker : IEventHandler
    {
        void OnUseLocker(PlayerUseLockerEvent ev);
    }

    public interface IHandlerScp914UpgradePickup : IEventHandler
    {
        void OnUpgradePickup(Scp914UpgradePickupEvent ev);
    }

    public interface IHandlerGenerateSeed : IEventHandler
    {
        void OnGenerateSeed(GenerateSeedEvent ev);
    }

    public interface IHandlerItemChangeDurability : IEventHandler
    {
        void OnChangeDurability(ChangeItemDurabilityEvent ev);
    }

    public interface IHandlerItemChangeAttachments : IEventHandler
    {
        void OnChangeAttachments(ChangeItemAttachmentsEvent ev);
    }

    public interface IHandlerDamageWindow : IEventHandler
    {
        void OnDamageWindow(DamageWindowEvent ev);
    }

    public interface IHandlerGrenadeExplode : IEventHandler
    {
        void OnGrenadeExplode(GrenadeExplodeEvent ev);
    }

    public interface IHandlerMapGenerated : IEventHandler
    {
        void OnMapGenerated(MapGeneratedEvent ev);
    }

    public interface IHandlerPreAuth : IEventHandler
    {
        void OnPreAuth(PreAuthEvent ev);
    }

    public interface IHandlerKick : IEventHandler
    {
        void OnKick(KickEvent ev);
    }

    public interface IHandlerMedicalDequip : IEventHandler
    {
        void OnDequipMedical(DequipMedicalItemEvent ev);
    }

    public interface IHandlerPlayerVerified : IEventHandler
    {
        void OnVerified(PlayerVerifiedEvent ev);
    }

    public interface IHandlerPlayerReceiveEffect : IEventHandler
    {
        void OnReceiveEffect(PlayerReceiveEffect ev);
    }

    public interface IHandlerActivateWorkStation : IEventHandler
    {
        void OnActivateWorkStation(WorkStationActivateEvent ev);
    }

    public interface IHandlerDeactivateWorkStation : IEventHandler
    {
        void OnDeactivateWorkStation(WorkStationDeactivateEvent ev);
    }

    public interface IHandlerChangeMuteStatus : IEventHandler
    {
        void OnChangeMuteStatus(ChangeMuteStatusEvent ev);
    }

    public interface IHandlerScp079SwitchCamera : IEventHandler
    {
        void OnSwitchCamera(Scp079SwitchCameraEvent ev);
    }

    public interface IHandlerScp079Recontain : IEventHandler
    {
        void OnRecontain(Scp079RecontainEvent ev);
    }

    public interface IHandlerScp096PryGate : IEventHandler
    {
        void OnPryGate(Scp096PryGateEvent ev);
    }

    public interface IHandlerPocketDie : IEventHandler
    {
        void OnPocketDie(PocketDieEvent ev);
    }

    public interface IHandlerLiftChangeStatus : IEventHandler
    {
        void OnLiftChangeStatus(LiftChangeStatusEvent ev);
    }

    public interface IHandlerLiftTeleport : IEventHandler
    {
        void OnLiftTeleport(LiftTeleportEvent ev);
    }

    public interface IHandlerIntercomCheckAllowedSpeak : IEventHandler
    {
        void OnIntercomCheck(IntercomCheckSpeakAllowedEvent ev);
    }

    public interface IHandlerAllowChangeItemCheck : IEventHandler
    {
        void OnAllowChangeItemCheck(AllowChangeItemCheckEvent ev);
    }

    public interface IHandlerScp049Interact : IEventHandler
    {
        void OnScp049Interact(Scp049InteractEvent ev);
    }

    public interface IHandlerSpawnDoor : IEventHandler
    {
        void OnDoorSpawn(SpawnDoorEvent ev);
    }

    public interface IHandlerSpawnItem : IEventHandler
    {
        void OnSpawnItem(SpawnItemEvent ev);
    }
}