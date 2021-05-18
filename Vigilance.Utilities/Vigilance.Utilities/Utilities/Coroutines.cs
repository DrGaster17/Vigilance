using System.Collections.Generic;

using UnityEngine;

using Vigilance.API;

using MEC;

namespace Vigilance.Utilities
{
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
}
