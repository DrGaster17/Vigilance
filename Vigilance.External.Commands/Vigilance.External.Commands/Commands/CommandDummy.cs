using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

using UnityEngine;

namespace Vigilance.External.Commands
{
    public class CommandDummy : ICommandHandler
    {
        public string Command => "dummy";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 4)
                return "Missing arguments!\nUsage: dummy <player/*/clear> <role> <item> <scale - X,Y,Z>";

            if (args[0] == "clear")
            {
                CommandUtils.ClearDummies();

                return "Dummies cleared.";
            }

            RoleType role = args[1].GetRole();
            ItemType item = args[2].GetItem();

            if (!StringUtils.TryParseVector(args[3], out Vector3? scale) || !scale.HasValue || scale.Value == null)
                return "Something went wrong while trying to parse scale - make sure to enter in a correct format, eg. - X,Y,Z\nExample: 5,5,5";

            if (args[0] == "*" || args[0] == "all")
            {
                PlayersList.List.ForEach(x =>
                {
                    CommandUtils.SpawnDummy(role, item, scale.Value, sender.Position, sender.Rotations);
                });

                return "Spawned a dummy at all players.";
            }
            else
            {
                Player player = args[0].GetPlayer();

                if (player == null)
                    return "Player not found.";

                CommandUtils.SpawnDummy(role, item, scale.Value, sender.Position, sender.Rotations);

                return "Spawned a dummy at your position";
            }
        }
    }
}
