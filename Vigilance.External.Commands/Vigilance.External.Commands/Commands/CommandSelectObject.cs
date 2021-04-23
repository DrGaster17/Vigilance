using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using System.Collections.Generic;

namespace Vigilance.External.Commands
{
    public class CommandSelectObject : ICommandHandler
    {
        public static Dictionary<Player, GameObject> Objects = new Dictionary<Player, GameObject>();

        public string Command => "selectobject";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            GameObject obj = sender.LookingAt;

            if (obj == null)
                return "Error: The object is null. Perhaps the object you're looking at doesn't have a collider.";

            if (!Objects.ContainsKey(sender))
            {
                Objects.Add(sender, obj);
                return "Selected object.";
            }
            else
            {
                Objects.Remove(sender);
                return "You are no longer able to select an object.";
            }
        }
    }
}
