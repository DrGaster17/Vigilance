﻿using System;
using Harmony;
using UnityEngine;
using CustomPlayerEffects;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp939PlayerScript), nameof(Scp939PlayerScript.CallCmdShoot))]
    public static class Scp939PlayerScript_CallCmdShoot
    {
        public static bool Prefix(Scp939PlayerScript __instance, GameObject target)
        {
            try
            {
                if (target == null || !__instance.iAm939 || __instance.cooldown > 0f 
                    || Vector3.Distance(target.transform.position, __instance.transform.position) >= __instance.attackDistance * 1.2f)
                    return false;

                __instance.cooldown = PluginManager.Config.Scp939KillCooldown;

                __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(PluginManager.Config.Scp939Damage, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp939, __instance._hub.queryProcessor.PlayerId), target, false, true);

                ReferenceHub hub = ReferenceHub.GetHub(target);

                if (hub != null && hub.playerEffectsController != null && PluginManager.Config.Scp939Amnesia)
                    hub.playerEffectsController.EnableEffect<Amnesia>(3f, true);

                __instance.RpcShoot();

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp939PlayerScript_CallCmdShoot), e);
                return true;
            }
        }
    }
}
