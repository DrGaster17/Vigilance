using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandSelectTesla : ICommandHandler
    {
        public static Dictionary<Player, Tesla> SelectedGates = new Dictionary<Player, Tesla>();

        public string Command => "selecttesla";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedGates.ContainsKey(sender))
            {
                SelectedGates.Add(sender, null);
                return "You are now able to select a tesla gate by triggering one.";
            }
            else
            {
                SelectedGates.Remove(sender);
                return "You are no longer able to select a tesla gate.";
            }
        }
    }
}
