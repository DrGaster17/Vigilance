using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;

using System.Linq;

namespace Vigilance.Plugins.Commands
{
    public class CommandPlayers : ICommandHandler
    {
        public string Command => "players";
        public string[] Aliases => new string[] { "plys", "plylist", "playerlist" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (PlayersList.List.Count < 1)
                return "There aren't any players on the server.";

            string str = $"Online Players ({PlayersList.List.Count}):\n";

            PlayersList.List.OrderBy(x => x.PlayerId).ToList().ForEach(x => str += $"{x}\n");

            return str;
        }
    }
}
