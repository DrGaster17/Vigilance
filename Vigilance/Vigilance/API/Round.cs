using System;
using GameCore;
using Respawning;
using Respawning.NamingRules;
using Vigilance.API.Enums;
using Vigilance.External.Utilities;

namespace Vigilance.API
{
    public static class Round
    {
        public static TimeSpan ElapsedTime => RoundStart.RoundLenght;
        public static DateTime StartedTime => DateTime.Now - ElapsedTime;

        public static bool IsStarted => RoundSummary.RoundInProgress();
        public static bool IsLocked { get => RoundSummary.RoundLock; set => RoundSummary.RoundLock = value; }
        public static bool IsLobbyLocked { get => RoundStart.LobbyLock; set => RoundStart.LobbyLock = value; }

        public static void Restart(bool fastRestart = true, bool overrideRestartAction = false, ServerStatic.NextRoundAction restartAction = ServerStatic.NextRoundAction.DoNothing)
        {
            var pStats = LocalComponents.PlayerStats;

            if (overrideRestartAction)
                ServerStatic.StopNextRound = restartAction;

            var oldValue = CustomNetworkManager.EnableFastRestart;

            CustomNetworkManager.EnableFastRestart = fastRestart;

            if (pStats != null)
            {
                pStats.Roundrestart();
            }
            else
            {
                PlayerStats.StaticChangeLevel(noShutdownMessage: true);
            }

            CustomNetworkManager.EnableFastRestart = oldValue;
        }

        public static void RestartSilently() => Restart(fastRestart: true, overrideRestartAction: true, restartAction: ServerStatic.NextRoundAction.DoNothing);

        public static bool End()
        {
            if (RoundSummary.singleton._keepRoundOnOne && PlayersList.Hubs.Count < 2)
                return false;

            if (IsStarted && !IsLocked)
            {
                RoundSummary.singleton.ForceEnd();
                return true;
            }

            return false;
        }

        public static void Start() => CharacterClassManager.ForceRoundStart();

        public static bool RoundLock { get => RoundSummary.RoundLock; set => RoundSummary.RoundLock = value; }
        public static bool LobbyLock { get => RoundStart.LobbyLock; set => RoundStart.LobbyLock = value; }
        public static bool FriendlyFire { get => ServerConsole.FriendlyFire; set => ServerConsole.FriendlyFire = value; }

        public static RoundState State { get; set; } 

        public static void AddUnit(string unit, SpawnableTeamType teamType = SpawnableTeamType.NineTailedFox)
        {
            SyncUnit syncUnit = new SyncUnit()
            {
                SpawnableTeam = (byte)teamType,
                UnitName = unit
            };

            RespawnManager.Singleton.NamingManager.AllUnitNames.Add(syncUnit);
        }

        public static void SetSpeed(float value)
        {
            ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier = value;
            ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier = value / 1.5f;
        }
    }
}
