using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdContain106))]
    public static class PlayerInteract_CallCmdContain106
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
				if (!__instance._playerInteractRateLimit.CanExecute(true) || ((__instance._hc.CufferId > 0 || __instance._hc.ForceCuff) && !PlayerInteract.CanDisarmedInteract))
					return false;

				if (!MapCache.LureSubjectContainer.allowContain || __instance._ccm.IsAnyScp() || __instance.ChckDis(MapCache.FemurBreaker.transform.position) 
					|| OneOhSixContainer.used || !__instance._ccm.IsAlive)
					return false;

				bool contained = false;

				foreach (ReferenceHub hub in ReferenceHub.Hubs.Values)
                {
					if (hub.characterClassManager.CurClass == RoleType.Scp106 && !hub.characterClassManager.GodMode)
                    {
						Player scp = PlayersList.GetPlayer(hub);
						Player killer = PlayersList.GetPlayer(__instance._hub);

						if (scp == null || killer == null)
							continue;

						if (killer.PlayerLock)
							continue;

						SCP106ContainEvent ev = new SCP106ContainEvent(killer, scp, true);
						EventManager.Trigger<IHandlerScp106Contain>(ev);

						if (!ev.Allow)
							return false;

						contained = true;
						hub.scp106PlayerScript.Contain(__instance._hub);
                    }
                }

				if (contained)
				{
					__instance.RpcContain106(__instance.gameObject);
					OneOhSixContainer.used = true;
				}

				__instance.OnInteract();
				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdContain106), e);
                return true;
            }
        }
    }
}
