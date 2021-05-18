using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public class CommandGhost : ICommandHandler
    {
        public string Command => "ghost";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
            {
                sender.IsInvisible = !sender.IsInvisible;
                return $"You were {(sender.IsInvisible ? "ghosted" : "unghosted")}.";
            }
            else
            {
                Player player = args.Combine().GetPlayer();

                if (player == null)
                    return "Player not found.";

                player.IsInvisible = !player.IsInvisible;
                return $"{(player.IsInvisible ? "Ghosted" : "Unghosted")} {player.Nick}.";
            }
        }
    }
}
