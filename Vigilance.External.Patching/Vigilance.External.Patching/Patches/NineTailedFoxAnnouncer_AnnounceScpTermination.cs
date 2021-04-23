using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
    public static class NineTailedFoxAnnouncer_AnnounceScpTermination
    {
        public static bool Prefix(NineTailedFoxAnnouncer __instance, Role scp, PlayerStats.HitInfo hit, string groupId)
        {
            try
            {
                AnnounceSCPTerminationEvent ev = new AnnounceSCPTerminationEvent(string.IsNullOrEmpty(hit.Attacker) ? null : PlayersList.GetPlayer(hit.Attacker), scp, hit, groupId, true);
                EventManager.Trigger<IHandlerAnnounceScpTermination>(ev);
                return ev.Allow;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(NineTailedFoxAnnouncer_AnnounceScpTermination), e);
                return true;
            }
        }
    }
}
