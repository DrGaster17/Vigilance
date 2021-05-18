using System;
using PlayableScps;
using Harmony;
using UnityEngine;
using Mirror;
using PlayableScps.Messages;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargePlayer))]
    public static class Scp096_ChargePlayer
    {
        public static bool Prefix(Scp096 __instance, ReferenceHub player)
        {
            try
            {
				if (player.characterClassManager.IsAnyScp())
					return false;

				if (Physics.Linecast(__instance.Hub.transform.position, player.transform.position, LayerMask.GetMask(new string[]
				{
					"Default",
					"Door",
					"Glass"
				})))
				{
					return false;
				}

				bool isTarget = __instance._targets.Contains(player);

				if (!isTarget && PluginManager.Config.Scp096CanKillOnlyTargets)
					return false;

				if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(isTarget ? PluginManager.Config.Scp096TargetDamage : PluginManager.Config.Scp096NonTargetDamage, player.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId), player.gameObject, false, true))
				{
					__instance._targets.Remove(player);
					NetworkServer.SendToClientOfPlayer(__instance.Hub.characterClassManager.netIdentity, new Scp096HitmarkerMessage(1.35f));
					NetworkServer.SendToAll(default(Scp096OnKillMessage), 0);
				}

				if (isTarget)
				{
					__instance.EndChargeNextFrame();
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp096_ChargePlayer), e);
                return true;
            }
        }
    }
}
