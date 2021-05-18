using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public class CommandChangeUnit : ICommandHandler
    {
        public string Command => "changeunit";
        public string[] Aliases => new string[] { "cu" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: changeunit (player) <unit>";

            Player player = args.Length < 2 ? sender : args[0].GetPlayer();

            if (player == null)
                return "Player not found.";

            string unit = args.SkipWords(player == sender ? 0 : 1);

            player.NtfUnit = unit;

            return $"Unit of {player.Nick} was set to {unit}";
        }
    }
}
