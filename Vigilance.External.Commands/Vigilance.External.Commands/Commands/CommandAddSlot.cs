using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;

namespace Vigilance.External.Commands
{
    public class CommandAddSlot : ICommandHandler
    {
        public string Command => "addslot";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: addslot <userId>";

            string id = args.Combine();

            if (Server.AddReservedSlot(id))
                return $"Succesfully added a reserved slot for {id}.";

            return $"There has been an error while adding a reserved slot for {id}.";
        }
    }
}
