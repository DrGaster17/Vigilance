using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.External.Extensions;

using Mirror;

namespace Vigilance.External.Commands
{
    public class CommandWorkStationAction : ICommandHandler
    {
        public string Command => "workstationaction";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: workstationaction <action>\nUse \"workstationaction list\" to get a list of all actions.";

            if (!CommandSelectWorkStation.SelectedStations.TryGetValue(sender, out Workstation w) || w == null)
                return "You have to select a work station first.";

            if (args[0] == "info")
            {
                string s = "WorkStation Info:\n";
                s += $"ID: {w.GameObject.GetInstanceID()}\n";
                s += $"Room: {w.Room.Type}\n";
                s += $"Tablet Connected: {w.IsConnected}\n";
                s += $"Locked: {w.IsLocked}\n";
                s += $"Max Distance: {w.MaxDistance}\n";
                s += $"Tablet Owner: {(w.TabletOwner == null ? "None" : $"{w.TabletOwner}")}";

                return s;
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
                    w.Scale = sender.Scale;
                    return "Scale set to your scale.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Scale must be a valid Vector3 in correct format - X,Y,Z.";

                w.Scale = vec.Value;

                return $"Scale set.";
            }

            if (args[0] == "delete")
            {
                NetworkServer.Destroy(w.GameObject);
                return "Deleted.";
            }

            if (args[0] == "list")
            {
                return "setscale, setrot, setpos, info, delete";
            }

            return "Missing arguments!\nUsage: workstationaction <action>\nUse \"workstationaction list\" to get a list of all actions.";
        }
    }
}
