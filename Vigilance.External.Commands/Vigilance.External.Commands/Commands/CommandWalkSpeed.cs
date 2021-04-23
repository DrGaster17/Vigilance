using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandWalkSpeed : ICommandHandler
    {
        public string Command => "walkspeed";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
                return "Missing arguments!\nUsage: walkspeed <player> <speed>";

            if (!float.TryParse(args[1], out float speed))
                return "Speed must be a valid integer.";

            Player player = args[0].GetPlayer();

            if (player == null)
                return "Player not found.";

            player.WalkSpeedMultiplier = speed;
            player.ReferenceHub.playerMovementSync.WhitelistPlayer = true;
            player.ReferenceHub.playerMovementSync.NoclipWhitelisted = true;

            return $"Walk Speed Multiplier of {player.Nick} was set to {speed}";
        }
    }
}