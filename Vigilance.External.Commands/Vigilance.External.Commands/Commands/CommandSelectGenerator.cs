using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandSelectGenerator : ICommandHandler
    {
        public static Dictionary<Player, Generator> SelectedGenerators = new Dictionary<Player, Generator>();

        public string Command => "selectgenerator";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedGenerators.ContainsKey(sender))
            {
                SelectedGenerators.Add(sender, null);
                return "You are now able to select a generator by interacting with one.";
            }
            else
            {
                SelectedGenerators.Remove(sender);
                return "You are no longer able to select a generator.";
            }
        }
    }
}