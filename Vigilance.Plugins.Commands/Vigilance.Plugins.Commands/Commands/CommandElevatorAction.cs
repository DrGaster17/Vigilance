using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public class CommandElevatorAction : ICommandHandler
    {
        public string Command => "elevatoraction";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: elevatoraction <action>\nUse \"elevatoraction list\" to get a list of all actions.";

            if (!CommandSelectElevator.SelectedLifts.TryGetValue(sender, out Elevator el) || el == null)
                return "You have to select an elevator first.";

            if (args[0] == "info")
            {
                string s = "Elevator Info:\n";

                s += $"ID: {el.GameObject.GetInstanceID()}\n";
                s += $"Name: {el.Name}\n";
                s += $"Type: {el.Type}\n";
                s += $"Locked: {el.IsLocked}\n";
                s += $"Lockable: {el.Lockable}\n";
                s += $"MaxDistance: {el.MaxDistance}\n";
                s += $"Moving Speed: {el.MovingSpeed}\n";
                s += $"Operative: {el.Operative}\n";
                s += $"Position: {el.Position.AsString()}\n";
                s += $"Room: {el.Room.Type}\n";
                s += $"Status: {el.Status}";

                return s;
            }

            if (args[0] == "lock")
            {
                el.Lock();
                return "Elevator locked.";
            }

            if (args[0] == "unlock")
            {
                el.Unlock();
                return "Elevator unlocked.";
            }

            if (args[0] == "disallow")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: elevatoraction disallow <player>";

                Player player = args[1].GetPlayer();

                if (player == null)
                    return "Player not found.";

                el.DisallowedPlayers.Add(player);
                return $"Disallowed {player.Nick} from using this elevator.";
            }

            if (args[0] == "allow")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: elevatoraction allow <player>";

                Player player = args[1].GetPlayer();

                if (player == null)
                    return "Player not found.";

                el.DisallowedPlayers.Remove(player);
                return $"Allowed {player.Nick} using this elevator.";
            }

            if (args[0] == "distance")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"elevatoraction distance <amount>\"";

                if (!float.TryParse(args[1], out float amount))
                    return "\"amount\" must be a valid integer.";

                el.MaxDistance = amount;

                return $"Distance set to {amount}";
            }

            if (args[0] == "speed")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"elevatoraction speed <speed>\"";

                if (!float.TryParse(args[1], out float speed))
                    return "\"speed\" must be a valid integer.";

                el.MovingSpeed = speed;

                return $"Speed set to {speed}";
            }

            if (args[0] == "use")
            {
                el.Use();
                return "Elevator used.";
            }

            if (args[0] == "list")
            {
                return "info, use, speed, distance, allow, disallow, unlock, lock";
            }

            return "Missing arguments!\nUsage: elevatoraction <action>\nUse \"elevatoraction list\" to get a list of all actions.";
        }
    }
}
