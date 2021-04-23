using System.Collections.Generic;
using Vigilance.API;
using Mirror;
using MapGeneration;
using Grenades;
using UnityEngine;
using System.Linq;

namespace Vigilance.External.Patching
{
    public static class PatchData
    {
        public static Dictionary<Player, List<Pickup>> Pickups = new Dictionary<Player, List<Pickup>>();
        public static Dictionary<Player, List<Ragdoll>> Ragdolls = new Dictionary<Player, List<Ragdoll>>();

        public static List<Pickup> Scp914Pickups = new List<Pickup>();
        public static List<PlayerStats.HitInfo> Hits = new List<PlayerStats.HitInfo>();

        public static HashSet<DoorSpawnpoint> DoorSpawnpoints = new HashSet<DoorSpawnpoint>();

        public static bool ServersideExplosion(EffectGrenade grenade)
        {
            if (grenade.serverGrenadeEffect != null)
            {
                Transform transform = grenade.transform;
                Object.Instantiate(grenade.serverGrenadeEffect, transform.position, transform.rotation);
            }

            ServerLogs.AddLog(ServerLogs.Modules.Logger, string.Concat(new string[]
            {
                "Player ",
                grenade._throwerName,
                "'s ",
                grenade.logName,
                " grenade exploded."
            }), ServerLogs.ServerLogType.GameEvent, false);

            return true;
        }

        public static void Clear()
        {
            Pickups.Clear();
            Ragdolls.Clear();
            Scp914Pickups.Clear();
            Hits.Clear();
            DoorSpawnpoints.Clear();
        }

        public static void Update914Pickups(Pickup pickup)
        {
            if (!Scp914Pickups.Contains(pickup))
                Scp914Pickups.Add(pickup);
            else
                Scp914Pickups.Remove(pickup);
        }

        public static void AddPickup(Player owner, Pickup pickup)
        {
            if (!Pickups.ContainsKey(owner))
                Pickups.Add(owner, new List<Pickup>());

            Pickups[owner].Add(pickup);
        }

        public static void RemovePickup(Player owner, Pickup pickup)
        {
            if (!Pickups.ContainsKey(owner))
                Pickups.Add(owner, new List<Pickup>());

            Pickups[owner].Remove(pickup);
        }

        public static void AddRagdoll(Player owner, Ragdoll ragdoll)
        {
            if (!Ragdolls.ContainsKey(owner))
                Ragdolls.Add(owner, new List<Ragdoll>());

            Ragdolls[owner].Add(ragdoll);
        }

        public static void RemoveRagdoll(Player owner, Ragdoll ragoll)
        {
            if (!Ragdolls.ContainsKey(owner))
                Ragdolls.Add(owner, new List<Ragdoll>());

            Ragdolls[owner].Remove(ragoll);
        }

        public static void OnPlayerLeave(Player player)
        {
            if (Ragdolls.ContainsKey(player))
            {
                if (PluginManager.Config.RemoveRagdollsOnDisconnect)
                {
                    Ragdolls[player].ForEach((x) => NetworkServer.Destroy(x.gameObject));
                }

                Ragdolls[player].Clear();
                Ragdolls.Remove(player);
            }

            if (Pickups.ContainsKey(player))
            {
                if (PluginManager.Config.RemovePickupsOnDisconnect)
                {
                    Pickups[player].ForEach((x) => x.Delete());
                }

                Pickups[player].Clear();
                Pickups.Remove(player);
            }
        }
    }
}
