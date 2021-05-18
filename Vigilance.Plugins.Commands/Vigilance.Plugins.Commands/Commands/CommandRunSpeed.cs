using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;
using Vigilance.Commands;

namespace Vigilance.Plugins.Commands
{
    public class CommandRunSpeed : ICommandHandler
    {
        public string Command => "runspeed";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
                return "Missing arguments!\nUsage: runspeed <player> <speed>";

            if (!float.TryParse(args[1], out float speed))
                return "Speed must be a valid integer.";

            Player player = args[0].GetPlayer();

            if (player == null)
                return "Player not found.";

            player.RunSpeedMultiplier = speed;
            player.ReferenceHub.playerMovementSync.WhitelistPlayer = true;
            player.ReferenceHub.playerMovementSync.NoclipWhitelisted = true;

            return $"Run Speed Multiplier of {player.Nick} was set to {speed}";
        }
    }
}