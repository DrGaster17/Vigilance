using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.ClearTarget))]
    public static class Handcuffs_ClearTarget
    {
        public static bool Prefix(Handcuffs __instance)
        {
            try
            {
                foreach (Player player in PlayersList.List)
                {
                    if (player.CufferId == __instance.MyReferenceHub.queryProcessor.PlayerId)
                    {
                        Player cuffer = PlayersList.GetPlayer(__instance.MyReferenceHub);

                        if (player == null || cuffer == null)
                            return true;

                        if (cuffer.PlayerLock)
                            return false;

                        UncuffEvent ev = new UncuffEvent(player, cuffer, true);
                        EventManager.Trigger<IHandlerUncuff>(ev);

                        if (!ev.Allow)
                            return false;

                        player.CufferId = -1;
                        return false;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Handcuffs_ClearTarget), e);
                return true;
            }
        }
    }
}
