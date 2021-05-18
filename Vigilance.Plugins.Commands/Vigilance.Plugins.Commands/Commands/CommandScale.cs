using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

using UnityEngine;

namespace Vigilance.Plugins.Commands
{
    public class CommandScale : ICommandHandler
    {
        public string Command => "scale";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: (player) <scale>";

            Player player = args.Length < 2 ? sender : args[0].GetPlayer();

            if (player == null)
                return "Player not found.";

            if (!StringUtils.TryParseVector(player == sender ? args[0] : args[1], out Vector3? scale))
                return "Something went wrong while trying to parse scale - make sure to enter in a correct format, eg. - X,Y,Z\nExample: 5,5,5";

            player.Scale = scale.Value;

            return $"Scale of {player.Nick} was set to {scale.Value.AsString()}";
        }
    }
}
