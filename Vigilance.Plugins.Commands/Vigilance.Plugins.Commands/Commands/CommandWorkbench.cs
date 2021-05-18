using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

using UnityEngine;

namespace Vigilance.Plugins.Commands
{
    public class CommandWorkbench : ICommandHandler
    {
        public string Command => "workbench";
        public string[] Aliases => new string[] { "swb" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 2) 
				return "Missing arguments!\nUsage: workbench <player/*/clear> <scale>";

			if (!StringUtils.TryParseVector(args[1], out Vector3? scale))
				return "Something went wrong while trying to parse scale - make sure to enter in a correct format, eg. - X,Y,Z\nExample: 5,5,5";

			if (args[0] == "clear")
            {
				CommandUtils.ClearStations();

				return "Cleared al WorkStations.";
            }

			if (args[0] == "*" || args[0] == "all")
			{
				PlayersList.List.ForEach(x => CommandUtils.SpawnWS(x.Position, x.Rotations, scale.Value));

				return "Succesfully spawned a workbench at all players.";
			}

			Player ply = args[0].GetPlayer();

			if (ply == null)
				ply = sender;

			CommandUtils.SpawnWS(ply.Position, ply.Rotations, scale.Value);

			return $"Succesfully spawned a workbench at {ply.Nick}";
		}
    }
}
