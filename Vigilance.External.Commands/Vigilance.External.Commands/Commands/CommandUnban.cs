using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

using System.Collections.Generic;

namespace Vigilance.External.Commands
{
    public class CommandUnban : ICommandHandler
    {
        public string Command => "unban";
        public string[] Aliases => new string[] { "ub" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: unban <arg>\n arg can be either the name or the Ip Adress or the UserID of the player you want to unban.";

            string text = args.Combine();

            List<BanDetails> bansToRemove = CommandUtils.GetBansFor(text);

            if (bansToRemove.Count < 1)
                return $"No bans for {text} were found.";

            CommandUtils.RemoveBans(bansToRemove);

            return StringUtils.BansToString(bansToRemove);
        }
    }
}
