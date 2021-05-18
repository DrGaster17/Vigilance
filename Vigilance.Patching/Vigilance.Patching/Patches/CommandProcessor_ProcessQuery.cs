using System;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using Harmony;
using RemoteAdmin;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static class CommandProcessor_ProcessQuery
    {
        public static bool Prefix(string q, CommandSender sender)
        {
            try
            {
                Player player = sender.GetPlayer();

                RemoteAdminCommandEvent ev = new RemoteAdminCommandEvent(player, q, "", true);
                EventManager.Trigger<IHandlerRemoteAdmin>(ev);

                if (!ev.Allow)
                {
                    if (!string.IsNullOrEmpty(ev.Response))
                        sender.RaReply($"{q.Split(' ')[0].ToUpper()}#{ev.Response}", true, true, "");
                    else
                        Log.Add("Vigilance.Patching.Patches.CommandProcessor.ProcessQuery", $"The \"{typeof(RemoteAdminCommandEvent).FullName}\" event has been disallowed by a server plugin, but the response is an empty string.", LogType.Warn);

                    return false;
                }

                if (CommandManager.Run(player, q, CommandType.RemoteAdmin, out string reply, out string color))
                {
                    sender.RaReply(reply, true, true, "");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(CommandProcessor_ProcessQuery), e);
                return true;
            }
        }
    }
}
