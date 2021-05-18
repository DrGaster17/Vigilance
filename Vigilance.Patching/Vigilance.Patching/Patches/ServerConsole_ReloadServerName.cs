using System;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public static class ServerConsole_ReloadServerName
    {
        public static void Postfix(ServerConsole __instance)
        {
            try
            {
                if (!PluginManager.Config.ShouldTrack)
                    return;

                ServerConsole._serverName += $"<color=#00000000><size=1>Vigilance v{PluginManager.Version}</size></color>";
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ServerConsole_ReloadServerName), e);
            }
        }
    }
}
