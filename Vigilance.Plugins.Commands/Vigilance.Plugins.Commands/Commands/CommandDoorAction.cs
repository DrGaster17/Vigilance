using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.Extensions;

using Interactables.Interobjects.DoorUtils;

using Mirror;

namespace Vigilance.Plugins.Commands
{
    public class CommandDoorAction : ICommandHandler
    {
        public string Command => "dooraction";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: dooraction <action>\nUse \"dooraction actions\" to get a list of all actions.";

            if (!CommandSelectDoor.SelectedDoors.TryGetValue(sender, out API.Door door) || door == null)
                return "You must select a door with the selectdoor command first.";

            if (args[0] == "actions")
            {
                return "open, close, state, delete, destroy, disallow, allow, info, lock, unlock, setpos, setrot, setscale";
            }

            if (args[0] == "open")
            {
                door.Open();
                return "Door opened.";
            }

            if (args[0] == "close")
            {
                door.Close();
                return "Door closed.";
            }

            if (args[0] == "state")
            {
                door.ChangeState();
                return "Door state changed.";
            }

            if (args[0] == "delete")
            {
                door.Delete();
                return "Door deleted.";
            }

            if (args[0] == "destroy")
            {
                if (!door.IsDestroyable)
                    return "This door cannot be destroyed.";

                door.Destroy();
                return "Door destroyed.";
            }

            if (args[0] == "disallow")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: dooraction disallow <player>";

                Player player = args[1].GetPlayer();

                if (player == null)
                    return "Player not found.";

                door.DisallowedPlayers.Add(player);
                return $"Disallowed {player.Nick} from using this door.";
            }

            if (args[0] == "allow")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: dooraction allow <player>";

                Player player = args[1].GetPlayer();

                if (player == null)
                    return "Player not found.";

                door.DisallowedPlayers.Remove(player);
                return $"Allowed {player.Nick} using this door.";
            }

            if (args[0] == "info")
            {
                string s = $"Door Info:\n";

                s += $"Door ID: {door.Id}\n";
                s += $"Door Type: {door.Type}\n";
                s += $"Door Name: {door.Name}\n";
                s += $"Room: {door.Room.Type}\n";
                s += $"Permissions: {door.Permissions}\n";
                s += $"Opened: {door.IsOpen}\n";
                s += $"Locked: {door.IsLocked}\n";
                s += $"Position: {door.Position.AsString()}\n";
                s += $"Scale: {door.Scale.AsString()}\n";

                s += $"Flags:\n";

                if (door.AllowBypassOverride)
                    s += $"     - Bypass Override\n";

                if (door.IsCheckpoint)
                    s += $"     - Checkpoint\n";

                if (door.IsDestroyable)
                    s += $"     - Destroyable\n";

                if (door.IsDestroyed)
                    s += $"     - Destroyed\n";

                if (door.ScpOverride)
                    s += $"     - SCP Override";

                return s;
            }   

            if (args[0] == "lock")
            {
                door.Lock();
                return "Door locked.";
            }

            if (args[0] == "unlock")
            {
                door.Unlock();
                return "Door unlocked.";
            }

            if (args[0] == "setpos")
            {
                if (args.Length < 2)
                {
                    door.Position = sender.Position;
                    return "Position set to your position.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Position must be a valid Vector3 in correct format - X,Y,Z.";

                door.Position = vec.Value;

                return $"Position set.";
            }

            if (args[0] == "setscale")
            {
                if (args.Length < 2)
                {
                    door.Scale = sender.Scale;
                    return "Scale set to your scale.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Scale must be a valid Vector3 in correct format - X,Y,Z.";

                door.Scale = vec.Value;

                return $"Scale set.";
            }

            if (args[0] == "setrot")
            {
                if (args.Length < 2)
                {
                    door.Rotation = sender.Rotations;
                    return "Rotation set to your rotation.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Rotation must be a valid Vector3 in correct format - X,Y,Z.";

                door.Rotation = Quaternion.Euler(vec.Value);

                return $"Rotation set.";
            }

            if (args[0] == "clone")
            {
                door.CloneAt(sender.Position, sender.Rotations, sender.Scale);

                return "Succesfully spawned a clone at your location.";
            }

            return "Missing arguments!\nUsage: dooraction <action>\nUse dooraction actions to get a list of all actions.";
        }
    }
}
