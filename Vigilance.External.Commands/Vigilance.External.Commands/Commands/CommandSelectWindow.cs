using System.Collections.Generic;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

namespace Vigilance.External.Commands
{
    public class CommandSelectWindow : ICommandHandler
    {
        public static Dictionary<Player, Window> SelectedWindows = new Dictionary<Player, Window>();

        public string Command => "selectwindow";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!SelectedWindows.ContainsKey(sender))
            {
                SelectedWindows.Add(sender, null);
                return "You are now able to select a window by shooting at one.";
            }
            else
            {
                SelectedWindows.Remove(sender);
                return "You are no longer able to select a window.";
            }
        }
    }
}
