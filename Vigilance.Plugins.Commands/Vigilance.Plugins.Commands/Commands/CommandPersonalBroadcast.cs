using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public class CommandPersonalBroadcast : ICommandHandler
    {
        public string Command => "pbc";
        public string[] Aliases => new string[1];
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
                return "Missing arguments!\nUsage: pbc <player> (time - defaults to 10 seconds) (monospaced - defaults to false) <text>";

            Player player = args[0].GetPlayer();

            if (player == null)
                return "Player not found.";

            bool parsed = int.TryParse(args[1], out int time);
            bool parsedBool = bool.TryParse(args[2], out bool mono);

            if (!parsed)
                time = 10;

            if (!parsedBool)
                mono = false;

            string text = args.SkipWords(parsed ? (parsedBool ? 3 : 2) : (parsedBool ? 2 : 1));

            player.Broadcast(text, time, mono);

            return $"\nTarget: {player.Nick}\nTime: {time} second(s)\nMonospaced: {mono}\nText: {text}";
        }
    }
}
