using System;
using Harmony;
using Vigilance.API;
using Vigilance.Utilities;
using RemoteAdmin;
using CommandSystem;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    public static class QueryProcessor_ProcessGameConsoleQuery
    {
        public static bool Prefix(QueryProcessor __instance, string query, bool encrypted)
        {
            try
            {
				Player sender = Server.PlayerList.GetPlayer(__instance._hub);
				if (sender == null) return true;
				string[] array = query.Split(' ');
				Handling.OnConsoleCommand(array[0], sender, true, out bool allow, out string rep, out string col);

				if (!allow)
                {
					__instance.GCT.SendToClient(__instance.connectionToClient, rep, col);
					return false;
                }

				ICommand command;
				if (QueryProcessor.DotCommandHandler.TryGetCommand(array[0], out command))
				{
					try
					{
						string str;
						command.Execute(array.Segment(1), __instance._sender, out str);
						__instance.GCT.SendToClient(__instance.connectionToClient, array[0].ToUpper() + "#" + str, "");
					}
					catch (Exception arg)
					{
						__instance.GCT.SendToClient(__instance.connectionToClient, array[0].ToUpper() + "# Command execution failed! Error: " + arg, "");
					}

					return false;
				}

				if (CommandManager.CallCommand(sender, array.Segment(1).Array, out string reply, out string color))
                {
					__instance.GCT.SendToClient(__instance.connectionToClient, reply, color);
					return false;
                }

				__instance.GCT.SendToClient(__instance.connectionToClient, "Command not found.", "red");
				return false;
			}
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }
}
