using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;
using Vigilance.Utilities;

using MEC;

namespace Vigilance.Plugins.Commands
{
    public class CommandRagdoll : ICommandHandler
    {
        public string Command => "ragdoll";
        public string[] Aliases => new string[1];
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: ragdoll <player/*> (role - defaults to your role) (amount - defaults to 10 ragdolls)";

            if (args[0] == "*" || args[0] == "all")
            {
                RoleType role = args.Length < 2 || args[1].GetRole() == RoleType.None ? sender.Role : args[1].GetRole();

                int amount = 10;

                if (args.Length > 2)
                    int.TryParse(args[2], out amount);

                PlayersList.List.ForEach(x => Timing.RunCoroutine(Coroutines.SpawnBodies(x.GetComponent<RagdollManager>(), (int)role, amount)));

                return $"Succesfully spawned {amount} ragdoll(s) of {role} at all players.";
            }    
            else
            {
                Player player = args[0].GetPlayer();

                if (player == null)
                    return "Player not found.";

                RoleType role = args.Length < 2 || args[1].GetRole() == RoleType.None ? sender.Role : args[1].GetRole();

                int amount = 10;

                if (args.Length > 2)
                    int.TryParse(args[2], out amount);

                Timing.RunCoroutine(Coroutines.SpawnBodies(player.GetComponent<RagdollManager>(), (int)role, amount));

                return $"Succesfully spawned {amount} ragdoll(s) of {role} at all players.";
            }
        }
    }
}
