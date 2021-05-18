using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;
using Vigilance.Utilities;

namespace Vigilance.Plugins.Commands
{
    public class CommandFlash : ICommandHandler
    {
        public string Command => "flash";
        public string[] Aliases => new string[] { "flash" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: flash <player/*> (amount)";

            int? amount = null;

            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out int i))
                    amount = i;
            }

            if (args[0] == "*" || args[0] == "all")
            {
                if (!amount.HasValue)
                {
                    PlayersList.List.ForEach(x => x.SpawnGrenade(GrenadeType.Flash));

                    return "Succesfully spawned a grenade at all players.";
                }
                else
                {
                    Common.Repeat(amount.Value, () =>
                    {
                        PlayersList.List.ForEach(x => x.SpawnGrenade(GrenadeType.Flash));
                    });

                    return $"Succesfully spawned {amount} grenade(s) at all players.";
                }
            }
            else
            {
                Player player = args[0].GetPlayer();

                if (player == null)
                    return "Player not found.";

                if (!amount.HasValue)
                {
                    player.SpawnGrenade(GrenadeType.Flash);

                    return $"Succesfully spawned a grenade at {player.Nick}.";
                }
                else
                {
                    Common.Repeat(amount.Value, () =>
                    {
                        player.SpawnGrenade(GrenadeType.Flash);
                    });

                    return $"Succesfully spawned {amount} grenade(s) at {player.Nick}.";
                }
            }
        }
    }
}
