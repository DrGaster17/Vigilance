using Vigilance.Commands;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

using UnityEngine;

namespace Vigilance.Plugins.Commands
{
    public class CommandPos : ICommandHandler
    {
        public string Command => "pos";
        public string[] Aliases => new string[] { };
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: pos (player) <get/set/add>";

            Player player = args.Length < 2 ? sender : args[0].GetPlayer();

            if (player == null)
                return $"Player not found.";

            if (player == sender)
            {
                switch (args[0])
                {
                    case "get":
                        {
                            string str = $"---- {player.Nick} ----\n";
                            str += $"Room: {(player.Room == null ? "None" : player.Room.Type.ToString())}\n";
                            str += $"Exact Position: {player.Position.AsString().Replace("[", "").Replace("]", "").Replace("|", "")}\n";
                            return str;
                        }

                    case "set":
                        {
                            if (args.Length < 2 || !StringUtils.TryParseVector(args[1], out Vector3? pos))
                                return "Missing arguments!\nUsage: pos (player) <get/set/add>";

                            player.Position = pos.Value;

                            return $"Succesfully updated your position.";
                        }

                    case "add":
                        {
                            if (args.Length < 2)
                                return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                            int type = -1;
                            float value = 0f;

                            if (args[1].StartsWith("X:"))
                            {
                                if (!float.TryParse(args[1].Replace("X:", ""), out value))
                                    return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                                type = 0;
                            }

                            if (args[1].StartsWith("Y:"))
                            {
                                if (!float.TryParse(args[1].Replace("Y:", ""), out value))
                                    return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                                type = 1;
                            }

                            if (args[1].StartsWith("Z:"))
                            {
                                if (!float.TryParse(args[1].Replace("Z:", ""), out value))
                                    return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                                type = 2;
                            }

                            if (type == -1 || value == 0f)
                                return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                            switch (type)
                            {
                                case 0:
                                    {
                                        player.Position = new Vector3(player.Position.x + value, player.Position.y, player.Position.z);
                                        return "Succesfully updated your position.";
                                    }

                                case 1:
                                    {
                                        player.Position = new Vector3(player.Position.x, player.Position.y + value, player.Position.z);
                                        return "Succesfully updated your position.";
                                    }
                                case 2:
                                    {
                                        player.Position = new Vector3(player.Position.x, player.Position.y, player.Position.z + value);
                                        return "Succesfully updated your position.";
                                    }
                                default:
                                    {
                                        return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";
                                    }
                            }
                        }
                    default:
                        {
                            return "Missing arguments!\nUsage: pos (player) <get/set/add>";
                        }
                }
            }
            else
            {
                switch (args[1])
                {
                    case "get":
                        {
                            string str = $"---- {player.Nick} ----\n";
                            str += $"Room: {(player.Room == null ? "None" : player.Room.Type.ToString())}\n";
                            str += $"Exact Position: {player.Position.AsString().Replace("[", "").Replace("]", "").Replace("|", "")}\n";
                            str += $"---- ---- ----";
                            return str;
                        }

                    case "set":
                        {
                            if (args.Length < 2 || !StringUtils.TryParseVector(args[1], out Vector3? pos))
                                return "Missing arguments!\nUsage: pos (player) <get/set/add>";

                            player.Position = pos.Value;

                            return $"Succesfully updated {player.Nick}'s position.";
                        }

                    case "add":
                        {
                            if (args.Length < 2)
                                return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                            int type = -1;
                            float value = 0f;

                            if (args[1].StartsWith("X:"))
                            {
                                if (!float.TryParse(args[1].Replace("X:", ""), out value))
                                    return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                                type = 0;
                            }

                            if (args[1].StartsWith("Y:"))
                            {
                                if (!float.TryParse(args[1].Replace("Y:", ""), out value))
                                    return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                                type = 1;
                            }

                            if (args[1].StartsWith("Z:"))
                            {
                                if (!float.TryParse(args[1].Replace("Z:", ""), out value))
                                    return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                                type = 2;
                            }

                            if (type == -1 || value == 0f)
                                return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";

                            switch (type)
                            {
                                case 0:
                                    {
                                        player.Position = new Vector3(player.Position.x + value, player.Position.y, player.Position.z);
                                        return $"Succesfully updated {player.Nick}'s position.";
                                    }

                                case 1:
                                    {
                                        player.Position = new Vector3(player.Position.x, player.Position.y + value, player.Position.z);
                                        return $"Succesfully updated {player.Nick}'s position.";
                                    }
                                case 2:
                                    {
                                        player.Position = new Vector3(player.Position.x, player.Position.y, player.Position.z + value);
                                        return $"Succesfully updated {player.Nick}'s position.";
                                    }
                                default:
                                    {
                                        return "Missing arguments!\nUsage: pos (player) add (X:) (Y:) (Z:)";
                                    }
                            }
                        }
                    default:
                        {
                            return "Missing arguments!\nUsage: pos (player) <get/set/add>";
                        }
                }
            }

        }
    }
}
