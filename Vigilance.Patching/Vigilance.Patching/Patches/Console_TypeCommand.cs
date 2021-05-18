using System;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using Harmony;
using Console = GameCore.Console;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Console), nameof(Console.TypeCommand))]
    public static class Console_TypeCommand
    {
        public static bool Prefix(Console __instance, string cmd, CommandSender sender = null)
        {
            try
            {
                ServerCommandEvent ev = new ServerCommandEvent(cmd, "", true);
                EventManager.Trigger<IHandlerServerCommand>(ev);

                if (!ev.Allow)
                {
                    if (!string.IsNullOrEmpty(ev.Response))
                    {
                        if (sender == null)
                            Log.Add("REPLY", ev.Response, ConsoleColor.Magenta);
                        else
                            sender.RaReply($"SERVER#{ev.Response}", true, true, "");
                    }
                    else
                    {
                        Patcher.LogWarn(typeof(Console_TypeCommand), $"The {typeof(ServerCommandEvent).FullName} was disallowed by a plugin, but the reply was empty.");
                    }

                    return false;
                }

                if (CommandManager.Run(sender == null || sender.SenderId == "SERVER CONSOLE" ? CompCache.Player : PlayersList.GetPlayer(sender.SenderId), cmd.Replace("@", "").Replace("/", "").Replace(".", ""), 
                    CommandType.ServerConsole, out string reply, out string color))
                {
                    if (sender == null)
                        Log.Add("REPLY", reply, ConsoleColor.Magenta);
                    else
                        sender.RaReply(reply, true, true, "");

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Console_TypeCommand), e);
                return true;
            }
        }
    }
}