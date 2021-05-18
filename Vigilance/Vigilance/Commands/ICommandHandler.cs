using Vigilance.API.Enums;
using Vigilance.API;

namespace Vigilance.Commands
{
    public interface ICommandHandler
    {
        string Command { get; }
        string[] Aliases { get; }
        CommandType[] Environments { get; }
        CommandPermission Permission { get; }

        string Execute(Player sender, string[] args);
    }
}
