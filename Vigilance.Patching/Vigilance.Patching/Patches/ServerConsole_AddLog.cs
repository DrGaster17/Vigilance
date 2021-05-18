using System;
using Vigilance.Custom.Items.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class ServerConsole_AddLog
    {
        public static bool Prefix(ServerConsole __instance, string q, ConsoleColor color = ConsoleColor.Gray)
        {
            if (q.StartsWith("Waiting for players"))
            {
                CustomGrenade.Handler.OnWaiting();
                WaitingForPlayersEvent wev = new WaitingForPlayersEvent();
                CustomItem.EventHandling.OnWaitingForPlayers(wev);
                EventManager.Trigger<IHandlerWaitingForPlayers>(wev);
            }

            ConsoleAddLogEvent ev = new ConsoleAddLogEvent(q, true);
            EventManager.Trigger<IHandlerConsoleAddLog>(ev);

            if (!ev.Allow)
                return false;

            ServerConsole.PrintOnOutputs(q);

            if (ServerStatic.ServerOutput == null)
                return false;

            ServerStatic.ServerOutput.AddLog(q, color);
            return false;
        }
    }
}
