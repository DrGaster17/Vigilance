using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;
using Vigilance.Custom.Items.API;

namespace Vigilance.External.Commands
{
    public class CommandCustomItem : ICommandHandler
    {
        public string Command => "customitem";
        public string[] Aliases => new string[] { "vi" };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: customitem <list/give/spawn>";

            if (args[0] == "list")
            {
                if (CustomItem.Registered.Count < 1)
                    return "There are not any registered Custom Items.";
                else
                {
                    string s = $"Custom Items ({CustomItem.Registered.Count}):\n";

                    foreach (CustomItem item in CustomItem.Registered)
                    {
                        s += $"---- {item.Name} ----\n";

                        s += $"Id: {item.Id}\n";
                        s += $"ItemId: {item.Type}\n";
                        s += $"Spawned Items: {item.Spawned.Count}\n";
                        s += $"Spawn Limit: {item.SpawnProperties.Limit}\n";
                        s += $"Spawn Points: {item.SpawnProperties.StaticSpawnPoints.Count + item.SpawnProperties.DynamicSpawnPoints.Count}\n";
                        s += $"Inventories: {item.InsideInventories.Count}\n";
                        s += $"Durability: {item.Durability}\n";
                        s += $"Description: {item.Description}\n";

                        s += $"---- ----- ---- -----\n";
                    }

                    return s;
                }
            }

            if (args[0] == "give")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: ci give <id> (player)";

                if (!int.TryParse(args[1], out int itemId))
                    return "An error occured while trying to parse ID.";

                Player player = args.Length > 2 ? args[2].GetPlayer() : sender;

                CustomItem item = CustomItem.Get(itemId);

                if (item == null)
                    return "Item with that ID does not exist.";

                item.Give(player);

                return $"Given {item.Name} to {player.Nick}";
            }

            if (args[0] == "spawn")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: ci spawn <id> (player)";

                if (!int.TryParse(args[1], out int itemId))
                    return "An error occured while trying to parse ID.";

                Player player = args.Length > 2 ? args[2].GetPlayer() : sender;

                CustomItem item = CustomItem.Get(itemId);

                if (item == null)
                    return "Item with that ID does not exist.";

                item.Spawn(player);

                return $"Spawned {item.Name} at {player.Nick}";
            }

            return "Missing arguments!\nUsage: customitem <list/give/spawn>";
        }
    }
}
