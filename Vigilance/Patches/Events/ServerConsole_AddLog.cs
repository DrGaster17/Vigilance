using Harmony;
using System;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class ServerConsole_AddLog
    {
        public static void Prefix(string q)
        {
            try
            {
                Vigilance.Utilities.Handling.OnConsoleAddLog(q);
                if (q == "Waiting for players...")
                    Vigilance.Utilities.Handling.OnWaitingForPlayers();
            }
            catch (Exception e)
            {
                Log.Add(nameof(ServerConsole.AddLog), e);             
            }
        }
    }
}
