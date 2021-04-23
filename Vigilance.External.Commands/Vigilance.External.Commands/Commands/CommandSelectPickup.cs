using System.Collections.Generic;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandSelectPickup : ICommandHandler
    {
        public static Dictionary<Player, Pickup> Pickups = new Dictionary<Player, Pickup>();

        public string Command => "selectpickup";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!Pickups.ContainsKey(sender))
            {
                Pickups.Add(sender, null);
                return "You are now able to select a pickup.";
            }
            else
            {
                Pickups.Remove(sender);
                return "You are no longer able to select a pickup.";
            }
        }
    }
}
