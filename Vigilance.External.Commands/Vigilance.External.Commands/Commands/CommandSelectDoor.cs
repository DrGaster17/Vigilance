using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandSelectDoor : ICommandHandler
    {
        public static Dictionary<Player, API.Door> SelectedDoors = new Dictionary<Player, API.Door>();

        public string Command => "selectdoor";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedDoors.ContainsKey(sender))
            {
                SelectedDoors.Add(sender, null);
                return "You are now able to select a door by interacting with one.";
            }
            else
            {
                SelectedDoors.Remove(sender);
                return "You are no longer able to select a door.";
            }
        }
    }
}
