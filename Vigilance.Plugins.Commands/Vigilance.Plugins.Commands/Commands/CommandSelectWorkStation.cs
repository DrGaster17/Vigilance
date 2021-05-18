using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.Plugins.Commands
{
    public class CommandSelectWorkStation : ICommandHandler
    {
        public static Dictionary<Player, Workstation> SelectedStations = new Dictionary<Player, Workstation>();

        public string Command => "selectworkstation";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedStations.ContainsKey(sender))
            {
                SelectedStations.Add(sender, null);
                return "You are now able to select a work station by interacting with one.";
            }
            else
            {
                SelectedStations.Remove(sender);
                return "You are no longer able to select a work station.";
            }
        }
    }
}