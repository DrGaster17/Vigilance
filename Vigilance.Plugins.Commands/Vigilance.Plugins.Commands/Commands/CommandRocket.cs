using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;
using Vigilance.Utilities;

namespace Vigilance.Plugins.Commands
{
    public class CommandRocket : ICommandHandler
    {
        public string Command => "rocket";
        public string[] Aliases => new string[] { "r" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 2)
				return "Missing arguments!\nUsage: rocket <player/*> (speed - defaults to 1)";

			if (!float.TryParse(args[1], out float speed))
				speed = 1f;

			if (args[0] == "*" || args[0] == "all")
			{
				PlayersList.List.ForEach(x =>
				{
					if (x.IsAlive)
                    {
						PlayerUtilities.Rocket(x, speed);
                    }
				});

				return $"Succesfully launched all players into space.";
			}

			Player player = args[0].GetPlayer();

			if (player == null) 
				return "Player not found.";

			if (!player.IsAlive) 
				return $"{player.Nick} is a spectator!";

			PlayerUtilities.Rocket(player, speed);

			return $"Succesfully launched {player.Nick} into space";
		}
    }
}
