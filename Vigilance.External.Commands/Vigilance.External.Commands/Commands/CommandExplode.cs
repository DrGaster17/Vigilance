using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

namespace Vigilance.External.Commands
{
    public class CommandExplode : ICommandHandler
    {
        public string Command => "explode";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: explode <player> (force - defaults to 1)";

            if (!float.TryParse(args[1], out float force))
                force = 1f;

            if (args[0] == "*" || args[0] == "all")
            {
                PlayersList.List.ForEach(x => x.Explode(force));
                return $"Spawned an explosion at all players.";
            }

            Player player = args[0].GetPlayer();

            if (player == null)
                return "Player not found";

            player.Explode(force);
            return $"Spawned an explosion at {player.Nick}.";
        }
    }
}
