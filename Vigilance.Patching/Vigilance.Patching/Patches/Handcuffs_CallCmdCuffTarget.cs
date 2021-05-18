using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using GameCore;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdCuffTarget))]
    public static class Handcuffs_CallCmdCuffTarget
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
				if (!__instance._interactRateLimit.CanExecute(true) || target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
					return false;

				Player cuffed = PlayersList.GetPlayer(target);
				Player cuffer = PlayersList.GetPlayer(__instance.MyReferenceHub);

				if (cuffed == null || cuffer == null)
					return true;

				if (cuffer.PlayerLock)
					return false;

				if (cuffer.CurrentItem.id != ItemType.Disarmer || cuffer.Role < RoleType.Scp173)
					return false;


				if (cuffed.CufferId < 0 && !__instance.ForceCuff && (PluginManager.Config.AllowCuffWhileHolding && cuffed.ReferenceHub.inventory.curItem != ItemType.None) || cuffed.ReferenceHub.inventory.curItem == ItemType.None)
				{
					HandcuffEvent ev = new HandcuffEvent(cuffed, cuffer, true);
					EventManager.Trigger<IHandlerHandcuff>(ev);
					Custom.Items.API.CustomItem.EventHandling.OnHandcuff(ev);

					if (!ev.Allow)
						return false;

					Team team = __instance.MyReferenceHub.characterClassManager.CurRole.team;
					Team team2 = cuffed.ReferenceHub.characterClassManager.CurRole.team;
					bool flag = false;

					if (team == Team.CDP)
					{
						if (team2 == Team.MTF || team2 == Team.RSC)
							flag = true;

					}
					else if (team == Team.RSC)
					{
						if (team2 == Team.CHI || team2 == Team.CDP)
							flag = true;

					}
					else if (team == Team.CHI)
					{
						if (team2 == Team.MTF || team2 == Team.RSC)
							flag = true;

						if (team2 == Team.CDP && ConfigFile.ServerConfig.GetBool("ci_can_cuff_class_d", false))
							flag = true;
					}
					else if (team == Team.MTF)
					{
						if (team2 == Team.CHI || team2 == Team.CDP)
							flag = true;

						if (team2 == Team.RSC && ConfigFile.ServerConfig.GetBool("mtf_can_cuff_researchers", false))
							flag = true;
					}

					if (flag)
					{
						if (team2 == Team.MTF && team == Team.CDP)
							__instance.MyReferenceHub.playerStats.TargetAchieve(__instance.MyReferenceHub.playerStats.connectionToClient, "tableshaveturned");

						__instance.ClearTarget();
						cuffed.CufferId = __instance.MyReferenceHub.queryProcessor.PlayerId;
					}
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Handcuffs_CallCmdCuffTarget), e);
                return true;
            }
        }
    }
}
