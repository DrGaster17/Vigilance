using UnityEngine;

using System.Linq;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.External.Extensions;

using Mirror;

namespace Vigilance.External.Commands
{
    public class CommandTeslaAction : ICommandHandler
    {
        public string Command => "teslaaction";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: teslaaction <action>\nUse \"teslaaction list\" to get a list of all actions.";

            if (args[0] == "spawn")
            {
                TeslaGate prefab = Object.FindObjectsOfType<TeslaGate>().FirstOrDefault();

                if (prefab != null)
                {
                    TeslaGate gate = Object.Instantiate(prefab);

                    if (gate != null)
                    {
                        gate.transform.localPosition = sender.Position;
                        gate.transform.localRotation = sender.Rotations;
                        gate.transform.localScale = sender.Scale;

                        gate.transform.position = sender.Position;
                        gate.transform.rotation = sender.Rotations;

                        gate.localPosition = sender.Position;

                        NetworkServer.Spawn(gate.gameObject);

                        return "Spawned a gate at your position.";
                    }

                    return "The TeslaGate object is null.";
                }

                return "The TeslaGate prefab is null.";
            }

            if (!CommandSelectTesla.SelectedGates.TryGetValue(sender, out Tesla t) || t == null)
                return "You have to select a tesla gate first.";

            if (args[0] == "info")
            {
                string s = "Tesla Info:\n";
                s += $"ID: {t.GameObject.GetInstanceID()}\n";
                s += $"Room: {t.Room.Type}\n";
                s += $"In Progress: {t.InProgress}\n";
                s += $"Disabled: {t.IsDisabled}\n";
                s += $"Size of Trigger: {t.SizeOfTrigger}\n";
                s += $"Position: {t.Position.AsString()}\n";
                s += $"Scale: {t.Scale.AsString()}";

                return s;
            }

            if (args[0] == "delete")
            {
                NetworkServer.Destroy(t.GameObject);
                return "Deleted.";
            }

            if (args[0] == "burst")
            {
                t.Burst();
                return "Done.";
            }

            if (args[0] == "anim")
            {
                t.PlayAnimation();
                return "Done.";
            }

            if (args[0] == "setpos")
            {
                if (args.Length < 2)
                {
                    t.Position = sender.Position;
                    return "Position set to your position.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Position must be a valid Vector3 in correct format - X,Y,Z.";

                t.Position = vec.Value;

                return $"Position set.";
            }

            if (args[0] == "setrot")
            {
                if (args.Length < 2)
                {
                    t.Rotation = sender.Rotations;
                    return "Rotation set to your rotation.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Rotation must be a valid Vector3 in correct format - X,Y,Z.";

                t.Rotation = Quaternion.Euler(vec.Value);

                return $"Rotation set.";
            }

            if (args[0] == "setscale")
            {
                if (args.Length < 2)
                {
                    t.Scale = sender.Scale;
                    return "Scale set to your scale.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Scale must be a valid Vector3 in correct format - X,Y,Z.";

                t.Scale = vec.Value;

                return $"Scale set.";
            }

            if (args[0] == "list")
                return "setscale, setrot, setpos, burst, anim, info";

            return "Missing arguments!\nUsage: teslaaction <action>\nUse \"teslaaction list\" to get a list of all actions.";
        }
    }
}
