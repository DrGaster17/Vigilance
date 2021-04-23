using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;
using Vigilance.External.Utilities;

namespace Vigilance.External.Commands
{
    public class CommandList : ICommandHandler
    {
        public string Command => "list";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: list <item/role/damagetype/grenadetype/useridtype/team/zonetype/roomtype/ammotype/prefab/infoarea>";

			string s = "";
			if (args[0].ToLower() == "item")
			{
				foreach (ItemType item in EnumUtilities.GetValues<ItemType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "role")
			{
				foreach (RoleType item in EnumUtilities.GetValues<RoleType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "damagetype")
			{
				foreach (DamageType item in EnumUtilities.GetValues<DamageType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "grenadetype")
			{
				foreach (GrenadeType item in EnumUtilities.GetValues<GrenadeType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "useridtype")
			{
				foreach (UserIdType item in EnumUtilities.GetValues<UserIdType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "team")
			{
				foreach (Team item in EnumUtilities.GetValues<Team>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "zonetype")
			{
				foreach (ZoneType item in EnumUtilities.GetValues<ZoneType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "roomtype")
			{
				foreach (RoomType item in EnumUtilities.GetValues<RoomType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "ammotype")
			{
				foreach (AmmoType item in EnumUtilities.GetValues<AmmoType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "prefab")
			{
				foreach (PrefabType item in EnumUtilities.GetValues<PrefabType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item.GetName()}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "infoarea")
			{
				foreach (PlayerInfoArea item in EnumUtilities.GetValues<PlayerInfoArea>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			return "Missing arguments!\nUsage: list <item/role/damagetype/grenadetype/useridtype/teamtype/team/zonetype/roomtype/ammotype/prefab/infoarea>";
		}
    }
}
