using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.External.Extensions;

namespace Vigilance.External.Commands
{
    public class CommandLockerAction : ICommandHandler
    {
        public string Command => "lockeraction";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: lockeraction <action>\nUse \"lockeraction list\" to get a list of all actions.";

            if (!CommandSelectLocker.SelectedLockers.TryGetValue(sender, out API.Locker l) || l == null)
                return "You have to select a locker first.";

            if (args[0] == "info")
            {
                string s = "Locker Info:\n";

                s += $"ID: {l.LockerId}\n";
                s += $"Name: {l.Name}\n";
                s += $"Tag: {l.Tag}\n";
                s += $"Items: {l.Items.Count}\n";
                s += $"Chambers: {l.Chambers.Count}\n";
                s += $"Position: {l.Position.AsString()}\n";
                s += $"Room: {l.Room.Type}\n";
                s += $"Spawned: {l.Spawned}\n";
                s += $"Spawn On Open: {l.SpawnOnOpen}\n";
                s += $"Triggered: {l.TriggeredByDoor}";

                return s;
            }

            if (args[0] == "list")
                return "info";

            return "Missing arguments!\nUsage: lockeraction <action>\nUse \"elevatoraction list\" to get a list of all actions.";
        }
    }
}
