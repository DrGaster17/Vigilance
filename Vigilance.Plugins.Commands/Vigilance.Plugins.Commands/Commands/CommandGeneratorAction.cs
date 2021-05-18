using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.Extensions;

using Mirror;

namespace Vigilance.Plugins.Commands
{
    public class CommandGeneratorAction : ICommandHandler
    {
        public string Command => "generatoraction";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: generatoraction <action>\nUse \"generatoraction list\" to get a list of all actions.";

            if (!CommandSelectGenerator.SelectedGenerators.TryGetValue(sender, out Generator gem) || gem == null)
                return "You have to select a generator first.";

            if (args[0] == "info")
            {
                string s = "Generator Info:\n";

                s += $"ID: {gem.GameObject.GetInstanceID()}\n";
                s += $"Room: {gem.Room.Type}\n";
                s += $"Is Open: {gem.IsDoorOpen}\n";
                s += $"Tablet Connected: {gem.IsTabletConnected}\n";
                s += $"Is Unlocked: {gem.IsUnlocked}\n";
                s += $"Local Voltage: {gem.LocalVoltage}\n";
                s += $"Position: {gem.Position.AsString()}\n";
                s += $"Remaining Powerup: {gem.RemainingPowerup}";

                return s;
            }

            if (args[0] == "delete")
            {
                NetworkServer.Destroy(gem.GameObject);
                return "Generator deleted.";
            }

            if (args[0] == "close")
            {
                gem.Close();
                return "Generator closed.";
            }

            if (args[0] == "open")
            {
                gem.Open();
                return "Generator opened.";
            }

            if (args[0] == "unlock")
            {
                gem.Unlock();
                return "Generator unlock.";
            }

            if (args[0] == "eject")
            {
                gem.Eject();
                return "Tablet ejected.";
            }

            if (args[0] == "overcharge")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"generatoraction overcharge <time>\"";

                if (!float.TryParse(args[1], out float time))
                    return "\"time\" must be a valid integer.";

                gem.Overcharge(time, false);

                return $"Overcharge commencing ..";
            }

            if (args[0] == "setpos")
            {
                if (args.Length < 2)
                {
                    gem.Position = sender.Position;
                    return "Position set to your position.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Position must be a valid Vector3 in correct format - X,Y,Z.";

                gem.Position = vec.Value;

                return $"Position set.";
            }

            if (args[0] == "setrot")
            {
                if (args.Length < 2)
                {
                    gem.Rotation = sender.Rotations;
                    return "Rotation set to your rotation.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Rotation must be a valid Vector3 in correct format - X,Y,Z.";

                gem.Rotation = Quaternion.Euler(vec.Value);

                return $"Rotation set.";
            }

            if (args[0] == "setscale")
            {
                if (args.Length < 2)
                {
                    gem.Scale = sender.Scale;
                    return "Scale set to your scale.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Scale must be a valid Vector3 in correct format - X,Y,Z.";

                gem.Scale = vec.Value;

                return $"Scale set.";
            }

            if (args[0] == "list")
            {
                return "setscale, setpos, setrot, overcharge, eject, unlock, close, open, info";
            }

            return "Missing arguments!\nUsage: generatoraction <action>\nUse \"elevatoraction list\" to get a list of all actions.";
        }
    }
}
