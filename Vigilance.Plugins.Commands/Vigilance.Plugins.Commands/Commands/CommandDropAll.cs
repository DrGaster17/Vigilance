using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public class CommandDropAll : ICommandHandler
    {
        public string Command => "dropall";
        public string[] Aliases => new string[1];
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: dropall <player/*>";

            if (args[0] == "all" || args[0] == "*")
            {
                PlayersList.List.ForEach(x => x.DropAllItems());
                return "Dropped items for all players.";
            }
            else
            {
                Player player = args.Combine().GetPlayer();

                if (player == null)
                    return "Player not found.";

                player.DropAllItems();

                return $"Dropped all items for {player.Nick}";
            }
        }
    }
}
