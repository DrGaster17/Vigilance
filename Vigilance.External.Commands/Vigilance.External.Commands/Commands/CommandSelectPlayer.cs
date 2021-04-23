using System.Collections.Generic;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;
using Vigilance.External.Extensions;
using Vigilance.External.Commands.Components;

namespace Vigilance.External.Commands
{
    public class CommandSelectPlayer : ICommandHandler
    {
        public static Dictionary<Player, Player> SelectedPlayers = new Dictionary<Player, Player>();

        public string Command => "selectplayer";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: selectplayer <player/random>";

            Player player = null;

            if (args[0] == "random")
            {
                player = PlayersList.List[new System.Random().Next(PlayersList.Count)];
            }
            else
            {
                player = PlayersList.GetPlayer(args.Combine());
            }

            if (player == null)
                return "Player not found.";

            if (!sender.ReferenceHub.TryGetComponent(out Grabber g))
                g = sender.AddComponent<Grabber>();

            if (g != null)
                g.player = player.ReferenceHub;

            if (!SelectedPlayers.ContainsKey(sender))
            {
                SelectedPlayers.Add(sender, player);
            }
            else
            {
                SelectedPlayers[sender] = player;
            }

            return $"Selected {player.Nick}";
        }
    }
}
