using Harmony;
using System;
using Vigilance.API;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class PlayerStats_Roundrestart
    {
        public static void Postfix(PlayerStats __instance)
        {
			try
			{
                foreach (Player player in Server.Players) Vigilance.Utilities.Handling.OnPlayerLeave(player, out bool destroy);
                RagdollManager_SpawnRagdoll.Owners.Clear();
                RagdollManager_SpawnRagdoll.Ragdolls.Clear();
                Inventory_CallCmdDropItem.Pickups.Clear();
                Server.PlayerList.Reset();
                Vigilance.Utilities.Handling.OnRoundRestart();
			}
			catch (Exception e)
            {
				Log.Add(e);
            }
		}
    }
}
