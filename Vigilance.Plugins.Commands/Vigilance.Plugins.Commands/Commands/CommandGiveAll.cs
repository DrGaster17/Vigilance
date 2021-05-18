using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public class CommandGiveAll : ICommandHandler
    {
        public string Command => "giveall";
        public string[] Aliases => new string[1];
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: giveall <itemId>";

            ItemType item = args.Combine().GetItem();

            PlayersList.ForEach(x => x.AddItem(item));
            return $"Succesfully gave {item} to {PlayersList.List.Count} player(s).";
        }
    }
}
