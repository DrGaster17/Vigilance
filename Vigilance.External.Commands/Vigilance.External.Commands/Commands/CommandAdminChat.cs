using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

using System.Linq;
using System.Collections.Generic;

namespace Vigilance.External.Commands
{
    public class CommandAdminChat : ICommandHandler
    {
        public string Command => "abc";
        public string[] Aliases => new string[] { "@" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: abc (time - defaults to 10 seconds) <text>";

            List<Player> admins = PlayersList.Where(x => x.RemoteAdmin).ToList();

            if (admins.Count < 1)
                return "There are not any admins on the server.";

            bool parsed = int.TryParse(args[0], out int time);

            if (!parsed)
                time = 10;

            string text = parsed ? args.SkipWords(1) : args.Combine();

            for (int i = 0; i < admins.Count; i++)
            {
                var admin = admins[i];

                admin.Broadcast($"<b><color=red>[AdminChat]</color></b>\n<b>{sender.Nick}:</b>", time, true);
                admin.ShowHint($"<b>{text}</b>");
            }

            return $"Done! Your message was sent to {admins.Count} online administrators.\nTime: {time} second(s)\nText: {text}";
        }
    }
}
