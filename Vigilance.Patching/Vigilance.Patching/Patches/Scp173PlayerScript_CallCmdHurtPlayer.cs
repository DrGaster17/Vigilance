using System;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.CallCmdHurtPlayer))]
    public static class Scp173PlayerScript_CallCmdHurtPlayer
    {
        public static bool Prefix(Scp173PlayerScript __instance, GameObject target)
        {
            try
            {
				if (!__instance._interactRateLimit.CanExecute(true) || __instance._hub.characterClassManager.CurClass != RoleType.Scp173 
					|| !__instance.CanMove(true) || target == null)
					return false;

				ReferenceHub hub = ReferenceHub.GetHub(target);

				if (hub == null || !hub.characterClassManager.IsHuman())
					return false;

				if (Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, target.transform.position) >= 0.7f + __instance.boost_teleportDistance.Evaluate(__instance._ps.GetHealthPercent()))
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "173 HurtPlayer command rejected - the distance between player and the target was too big (code: VC1).", "gray");
					return false;
				}

				if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, target.transform.position, __instance._hub.weaponManager.raycastServerMask))
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, "173 HurtPlayer command rejected - collider found between you and the target (code: T3).", "gray");
					return false;
				}

				__instance.RpcSyncAudio();

				__instance._hub.characterClassManager.RpcPlaceBlood(target.transform.position, 0, 2.2f);
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(PluginManager.Config.Scp173Damage, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp173, __instance._hub.queryProcessor.PlayerId), target, false, true);

				__instance.TargetHitMarker(__instance.connectionToClient);

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp173PlayerScript_CallCmdHurtPlayer), e);
                return true;
            }
        }
    }
}
