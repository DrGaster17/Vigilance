using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandSelectElevator : ICommandHandler
    {
        public static Dictionary<Player, Elevator> SelectedLifts = new Dictionary<Player, Elevator>();

        public string Command => "selectelevator";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedLifts.ContainsKey(sender))
            {
                SelectedLifts.Add(sender, null);
                return "You are now able to select an elevator by interacting with one.";
            }
            else
            {
                SelectedLifts.Remove(sender);
                return "You are no longer able to select an elevator.";
            }
        }
    }
}