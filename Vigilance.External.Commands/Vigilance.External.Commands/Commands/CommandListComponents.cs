using Vigilance.API;
using Vigilance.API.Enums;

using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandListComponents : ICommandHandler
    {
        public string Command => "listcomponents";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            CommandUtils.ListAllComponentsToFile();
            return "All Components were listed to a text file.";
        }
    }
}
