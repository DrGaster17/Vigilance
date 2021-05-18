using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

using System.Linq;

namespace Vigilance.Plugins.Commands
{
    public class CommandTeleport : ICommandHandler
    {
        public string Command => "tpx";
        public string[] Aliases => new string[] { "tp" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
                return "Missing arguments!\nUsage: tpx <player1/*> <player2>";

            if (args[0] == "*" || args[0] == "all")
            {
                Player player = args[1].GetPlayer();

                if (player == null)
                    return "Player not found.";

                PlayersList.List.Where(x => x != player).ToList().ForEach(x => x.Teleport(player.Position));

                return $"Succesfully teleported all players to {player.Nick}";
            }
            else
            {
                Player player = args[0].GetPlayer();
                Player to = args[1].GetPlayer();

                if (player == null || to == null)
                    return "Player not found.";

                player.Teleport(to.Position);

                return $"Succesfully teleported {player.Nick} to {to.Nick}";
            }
        }
    }
}
