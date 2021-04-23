using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

namespace Vigilance.External.Commands
{
    public class CommandAddUnit : ICommandHandler
    {
        public string Command => "addunit";
        public string[] Aliases => new string[] { "au" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: addunit <unit>";

            string unit = args.Combine();

            Round.AddUnit(unit);
            Round.AddUnit(unit, Respawning.SpawnableTeamType.ChaosInsurgency);

            return $"Added unit: {unit}";
        }
    }
}
