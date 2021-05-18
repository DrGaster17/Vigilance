using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;

namespace Vigilance.Plugins.Commands
{
    public class CommandReload : ICommandHandler
    {
        public string Command => "reload";
        public string[] Aliases => new string[1];
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            Server.Reload(true);

            return "Succesfully reloaded all plugins and configs.";
        }
    }
}
