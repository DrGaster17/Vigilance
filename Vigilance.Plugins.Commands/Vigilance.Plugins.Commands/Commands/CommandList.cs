using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;
using Vigilance.Utilities;

namespace Vigilance.Plugins.Commands
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
				foreach (ItemType item in Common.GetEnums<ItemType>())
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
				foreach (RoleType item in Common.GetEnums<RoleType>())
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
				foreach (DamageType item in Common.GetEnums<DamageType>())
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
				foreach (GrenadeType item in Common.GetEnums<GrenadeType>())
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
				foreach (UserIdType item in Common.GetEnums<UserIdType>())
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
				foreach (Team item in Common.GetEnums<Team>())
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
				foreach (ZoneType item in Common.GetEnums<ZoneType>())
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
				foreach (RoomType item in Common.GetEnums<RoomType>())
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
				foreach (AmmoType item in Common.GetEnums<AmmoType>())
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
				foreach (PrefabType item in Common.GetEnums<PrefabType>())
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
				foreach (PlayerInfoArea item in Common.GetEnums<PlayerInfoArea>())
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
