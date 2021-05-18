using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.Plugins.Commands
{
    public class CommandSelectLocker : ICommandHandler
    {
        public static Dictionary<Player, API.Locker> SelectedLockers = new Dictionary<Player, API.Locker>();

        public string Command => "selectlocker";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedLockers.ContainsKey(sender))
            {
                SelectedLockers.Add(sender, null);
                return "You are now able to select a locker by interacting with one.";
            }
            else
            {
                SelectedLockers.Remove(sender);
                return "You are no longer able to select a locker.";
            }
        }
    }
}