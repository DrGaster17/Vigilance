using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

using UnityEngine;

namespace Vigilance.Plugins.Commands
{
    public class CommandItemSize : ICommandHandler
    {
        public string Command => "itemsize";
        public string[] Aliases => new string[] { "is" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
                return "Missing arguments!\nUsage: itemsize (player) <itemType> <scale>";

            bool flag = args.Length > 2;

            ItemType item = args[flag ? 1 : 0].GetItem();
            Player player = flag ? args[0].GetPlayer() : sender;

            if (!StringUtils.TryParseVector(args[flag ? 3 : 2], out Vector3? vector))
                return "Something went wrong while trying to parse scale - make sure to enter in a correct format, eg. - X,Y,Z\nExample: 5,5,5";

            if (player == null || item == ItemType.None)
                return "Player or item not found.";

            CommandUtils.SpawnItemOfSize(item, vector.Value, player.Position, player.Rotations);

            return $"Spawned an item of type {item} at {player.Nick}.";
        }
    }
}
