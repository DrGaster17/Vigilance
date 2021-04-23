using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;

using Mirror;

using System.Linq;

namespace Vigilance.External.Commands
{
    public class CommandClearRagdolls : ICommandHandler
    {
        public string Command => "clearragdolls";
        public string[] Aliases => new string[] { "cr" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            Map.Ragdolls.ToList().ForEach(x => NetworkServer.Destroy(x.gameObject));

            return "All ragdolls deleted.";
        }
    }
}
