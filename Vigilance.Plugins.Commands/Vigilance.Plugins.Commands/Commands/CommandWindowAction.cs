using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Mirror;

namespace Vigilance.Plugins.Commands
{
    public class CommandWindowAction : ICommandHandler
    {
        public string Command => "windowaction";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: windowaction <action>\nUse \"windowaction list\" to get a list of all actions.";

            if (!CommandSelectWindow.SelectedWindows.TryGetValue(sender, out Window w) || w == null)
                return "You have to select a window first.";

            if (args[0] == "info")
            {
                string s = "Window Info:\n";

                s += $"ID: {w.InstanceId}\n";
                s += $"Health: {w.Health}\n";
                s += $"IsBroken: {w.IsBroken}\n";
                s += $"Room: {w.Room.Type}";

                return s;
            }

            if (args[0] == "delete")
            {
                NetworkServer.Destroy(w.GameObject);
                return "Deleted.";
            }

            if (args[0] == "break")
            {
                w.Break();
                return "Done.";
            }

            if (args[0] == "enableColliders")
            {
                w.EnableColliders();
                return "Colliders enabled.";
            }

            if (args[0] == "disableColliders")
            {
                w.DisableColliders();
                return "Colliders disabled.";
            }

            if (args[0] == "setpos")
            {
                if (args.Length < 2)
                {
                    w.Position = sender.Position;
                    return "Position set to your position.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Position must be a valid Vector3 in correct format - X,Y,Z.";

                w.Position = vec.Value;

                return $"Position set.";
            }

            if (args[0] == "setrot")
            {
                if (args.Length < 2)
                {
                    w.Rotation = sender.Rotations;
                    return "Rotation set to your rotation.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Rotation must be a valid Vector3 in correct format - X,Y,Z.";

                w.Rotation = Quaternion.Euler(vec.Value);

                return $"Rotation set.";
            }

            if (args[0] == "setscale")
            {
                if (args.Length < 2)
                {
                    NetworkServer.UnSpawn(w.GameObject);
                    w.GameObject.transform.localScale = sender.Scale;
                    NetworkServer.Spawn(w.GameObject);

                    return "Scale set to your scale.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Scale must be a valid Vector3 in correct format - X,Y,Z.";

                NetworkServer.UnSpawn(w.GameObject);
                w.GameObject.transform.localScale = vec.Value;
                NetworkServer.Spawn(w.GameObject);

                return $"Scale set.";
            }

            if (args[0] == "list")
            {
                return "setscale, setrot, setpos, enableColliders, disableColliders, break, info";
            }

            return "Missing arguments!\nUsage: windowaction <action>\nUse \"windowaction list\" to get a list of all actions.";
        }
    }
}
