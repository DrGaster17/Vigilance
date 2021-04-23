using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;

using MEC;

namespace Vigilance.External.Commands
{
    public class CommandRestart : ICommandHandler
    {
        public string Command => "restart";
        public string[] Aliases => new string[1];
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.ServerManagement;

        public string Execute(Player sender, string[] args)
        {
            Map.Broadcast("<b><color=red>The server is about to restart!</color></b>", 10, true);

            Timing.CallDelayed(2f, () =>
            {
                Server.Restart();
            });

            return "The server will restart in aprox. 3 seconds, please wait.";
        }
    }
}
