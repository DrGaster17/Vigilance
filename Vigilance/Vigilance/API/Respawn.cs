using System.Linq;
using Respawning;
using Vigilance.API.Enums;
using UnityEngine;

namespace Vigilance.API
{
    public static class Respawn
    {
        public static SpawnableTeamType NextKnownTeam => RespawnManager.Singleton.NextKnownTeam;

        public static int TimeUntilRespawn => Mathf.RoundToInt(RespawnManager.Singleton._timeForNextSequence - (float)RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds);
        public static bool IsSpawning => RespawnManager.Singleton._curSequence == RespawnManager.RespawnSequencePhase.PlayingEntryAnimations || RespawnManager.Singleton._curSequence == RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
        public static int NtfTickets => RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
        public static int ChaosTickets => RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency);

        public static RespawnEffectsController Controller => RespawnEffectsController.AllControllers.Where(controller => controller != null).FirstOrDefault();

        public static void PlayEffect(byte effect) => PlayEffects(new[] { effect });
        public static void PlayEffect(RespawnEffectType effect) => PlayEffects(new[] { effect });
        public static void PlayEffects(byte[] effects) => Controller.RpcPlayEffects(effects);
        public static void PlayEffects(RespawnEffectType[] effects) => PlayEffects(effects.Select(effect => (byte)effect).ToArray());

        public static void SummonNtfChopper() => PlayEffects(new RespawnEffectType[] { RespawnEffectType.SummonNtfChopper });
        public static void SummonChaosInsurgencyVan(bool playMusic = true)
        {
            PlayEffects(playMusic ? new RespawnEffectType[]
            {
                RespawnEffectType.PlayChaosInsurgencyMusic,
                RespawnEffectType.SummonChaosInsurgencyVan,
            }
            :
            new RespawnEffectType[]
            {
                RespawnEffectType.SummonChaosInsurgencyVan,
            });
        }

        public static bool GrantTickets(SpawnableTeamType team, int amount, bool overrideLocks = false) => RespawnTickets.Singleton.GrantTickets(team, amount, overrideLocks);

        public static void ForceWave(SpawnableTeamType team, bool playEffects = false)
        {
            RespawnManager.Singleton.ForceSpawnTeam(team);

            if (playEffects)
            {
                RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, team);
            }
        }
    }
}
