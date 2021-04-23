using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;

namespace Vigilance.External.Commands
{
    public class CommandForceEnd : ICommandHandler
    {
        public string Command => "forceend";
        public string[] Aliases => new string[] { "fe" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (Round.End())
                return "Done! Forced round end.";
            else
                return $"The round may not be started.";
        }
    }
}
