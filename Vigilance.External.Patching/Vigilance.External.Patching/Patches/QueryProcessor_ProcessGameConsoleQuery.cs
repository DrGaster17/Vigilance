using System;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;
using Vigilance.External.Extensions;
using Harmony;
using RemoteAdmin;
using CommandSystem;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    public static class QueryProcessor_ProcessGameConsoleQuery
    {
        public static bool Prefix(QueryProcessor __instance, string query, bool encrypted)
        {
            try
            {
				string[] array = query.Split(' ');
				Player sender = PlayersList.GetPlayer(__instance._hub);

				if (sender == null)
					return true;

				if (CommandManager.Run(sender, query, CommandType.PlayerConsole, out string reply, out string color))
				{
					__instance.GCT.SendToClient(__instance.connectionToClient, reply, color);
					return false;
				}

				if (QueryProcessor.DotCommandHandler.TryGetCommand(array[0], out ICommand command))
				{
					try
					{
						command.Execute(array.Segment(1), __instance._sender, out string str);
						__instance.GCT.SendToClient(__instance.connectionToClient, array[0].ToUpper() + "#" + str, "");
					}
					catch (Exception arg)
					{
						__instance.GCT.SendToClient(__instance.connectionToClient, array[0].ToUpper() + "# Command execution failed! Error: " + arg, "");
					}

					return false;
				}

				__instance.GCT.SendToClient(__instance.connectionToClient, "Command not found.", "red");
				return false;
            }
            catch (Exception e)
            {
                Log.Add("RemoteAdmin.QueryProcessor.ProcessGameConsoleQuery", e);
                return true;
            }
        }
    }
}
