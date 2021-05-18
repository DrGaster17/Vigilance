using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;

namespace Vigilance.Plugins.Commands
{
    public class CommandOban : ICommandHandler
    {
        public string Command => "oban";
        public string[] Aliases => new string[] { "ob" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!CommandUtils.IssueOfflineBan(args, sender.Nick, out string expiery, out string reason, out string error))
                return error;

            return $"Banned {args[0]} for {expiery}\nReason: {reason}";
        }
    }
}