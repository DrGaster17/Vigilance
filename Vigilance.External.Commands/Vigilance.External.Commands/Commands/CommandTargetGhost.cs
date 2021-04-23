using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

namespace Vigilance.External.Commands
{
    public class CommandTargetGhost : ICommandHandler
    {
        public string Command => "targetghost";
        public string[] Aliases => new string[] { "tg" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
            {
                if (args.Length < 1)
                    return "Missing arguments!\nUsage: targetghost <player>";

                Player target = args.Combine().GetPlayer();

                if (target == null)
                    return "Player not found.";

                if (sender.TargetGhosts.Contains(target))
                {
                    sender.TargetGhosts.Remove(target);
                    return $"Removed {target.Nick} from your targets.";
                }
                else
                {
                    sender.TargetGhosts.Add(target);
                    return $"Added {target.Nick} to your targets.";
                }
            }
            else
            {
                Player player = args[0].GetPlayer();
                Player target = args[1].GetPlayer();

                if (player == null || target == null)
                    return "Player not found.";

                if (player.TargetGhosts.Contains(target))
                {
                    player.TargetGhosts.Remove(target);
                    return $"Removed {target.Nick} from {player.Nick}'s targets.";
                }
                else
                {
                    player.TargetGhosts.Add(target);
                    return $"Added {target.Nick} to {player.Nick}'s targets.";
                }
            }
        }
    }
}
