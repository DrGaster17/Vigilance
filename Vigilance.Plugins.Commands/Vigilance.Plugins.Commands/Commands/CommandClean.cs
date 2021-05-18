using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;
using Vigilance.Utilities;

namespace Vigilance.Plugins.Commands
{
	public class CommandClean : ICommandHandler
	{
		public string Command => "clean";
		public string[] Aliases => new string[1];
		public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
		public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return "Missing arguments!\nUsage: clean <itemId/all>";

			if (args[0].ToLower() == "*" || args[0].ToLower() == "all")
			{
				foreach (Pickup pickup in MapUtilities.FindObjects<Pickup>()) 
					pickup.Delete();

				return "Succesfully cleared all items.";
			}
			else
			{
				ItemType item = args[0].GetItem();

				foreach (Pickup pickup in MapUtilities.FindObjects<Pickup>())
				{
					if (pickup.ItemId == item)
					{
						pickup.Delete();
					}
				}

				return $"Succesfully cleared all {item}s!";
			}
		}
	}
}
