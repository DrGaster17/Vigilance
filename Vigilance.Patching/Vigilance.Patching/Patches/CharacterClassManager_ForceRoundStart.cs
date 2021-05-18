using System;
using System.Linq;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using Vigilance.Custom.Items.API;
using Harmony;
using GameCore;
using MapGeneration;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ForceRoundStart))]
    public static class CharacterClassManager_ForceRoundStart
    {
        public static bool Prefix(CharacterClassManager __instance, ref bool __result)
        {
            try
            {
                MapUtilities.OnRoundStart();
                CustomItem.EventHandling.OnRoundStart();

                RoundStartEvent ev = new RoundStartEvent();
                EventManager.Trigger<IHandlerRoundStart>(ev);

                OneOhSixContainer.used = false;

                ServerLogs.AddLog(ServerLogs.Modules.Logger, "Round has been started.", ServerLogs.ServerLogType.GameEvent);
                ServerConsole.AddLog($"New round has been started with {ev.Players.Count()} players.");

                RoundStart.singleton.NetworkTimer = -1;
                RoundStart.RoundStartTimer.Restart();

                foreach (BreakableWindow window in MapUtilities.FindObjects<BreakableWindow>())
                {
                    window.health = PluginManager.Config.WindowHealth == -1 ? int.MaxValue : PluginManager.Config.WindowHealth;
                }

                __result = true;
                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(CharacterClassManager_ForceRoundStart), e);
                __result = false;
                return true;
            }
        }
    }
}
