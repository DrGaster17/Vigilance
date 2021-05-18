using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    public static class PlayerInteract_OnInteract
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (PluginManager.Config.ShouldKeepScp268)
                    return false;

                Player player = PlayersList.GetPlayer(__instance._hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                PlayerInteractEvent ev = new PlayerInteractEvent(player, true);
                EventManager.Trigger<IHandlerPlayerInteract>(ev);

                if (!ev.Allow)
                    return false;

                __instance._scp268.ServerDisable();

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_OnInteract), e);
                return true;
            }
        }
    }
}
