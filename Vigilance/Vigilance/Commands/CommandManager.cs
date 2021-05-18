using System;
using System.Collections.Generic;
using System.Linq;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.API.Plugins;
using Vigilance.API.Configs;

namespace Vigilance.Commands
{
    public static class CommandManager
    {
        private static Dictionary<IPlugin<IConfig>, List<ICommandHandler>> _commands = new Dictionary<IPlugin<IConfig>, List<ICommandHandler>>();
        private static List<ICommandHandler> _allCommands = new List<ICommandHandler>();

        public static void Register(IPlugin<IConfig> plugin, ICommandHandler handler)
        {
            if (!_commands.ContainsKey(plugin))
                _commands.Add(plugin, new List<ICommandHandler>());

            _allCommands.Add(handler);
            _commands[plugin].Add(handler);
        }

        public static void Remove(IPlugin<IConfig> plugin, string command)
        {
            if (!_commands.ContainsKey(plugin)) 
                _commands.Add(plugin, new List<ICommandHandler>());

            foreach (ICommandHandler handler in _commands[plugin])
            {
                if (handler.Command.ToLower() == command.ToLower())
                    _commands[plugin].Remove(handler);
            }
        }

        public static void RemoveAll(IPlugin<IConfig> plugin)
        {
            if (!_commands.ContainsKey(plugin)) 
                _commands.Add(plugin, new List<ICommandHandler>());

            _commands[plugin].Clear();
        }

        public static ICommandHandler Filter(string filter, FilterType type)
        {
            string filterLowered = filter.ToLower();

            if (type == FilterType.Normal || type == FilterType.Low)
            {
                foreach (ICommandHandler handler in _allCommands)
                {
                    string commandLowered = handler.Command.ToLower();

                    if (commandLowered == filterLowered)
                        return handler;
                }
            }

            if (type == FilterType.Exact)
            {
                foreach (ICommandHandler handler in _allCommands)
                {
                    string commandLowered = handler.Command.ToLower();

                    if (commandLowered == filterLowered)
                        return handler;

                    if (handler.Aliases != null && handler.Aliases.Length >= 1)
                    { 
                        foreach (string alias in handler.Aliases)
                        {
                            if (string.IsNullOrEmpty(alias))
                                continue;

                            string aliasLowered = alias.ToLower();

                            if (aliasLowered == filterLowered)
                                return handler;
                        }
                    }
                }
            }

            return null;
        }

        public static bool Run(Player sender, string cmdLine, CommandType type, out string reply, out string color)
        {
            string[] array = cmdLine.Split(' ');
            string command = array[0];
            IEnumerable<string> args = array.Skip(1);
            ICommandHandler handler = Filter(command, FilterType.Exact);

            if (handler == null)
            {
                reply = $"{command.ToUpper()}#Command not found.";
                color = "red";
                return false;
            }

            if (!handler.Environments.Contains(type))
            {
                reply = $"{command.ToUpper()}#This command cannot be run from the {type}!";
                color = "red";
                return true;
            }

            if (!CalculatePermissions(sender, handler.Permission))
            {
                reply = $"{command.ToUpper()}#You are not allowed to use this command!\nMissing \"{handler.Permission}\" permission.";
                color = "red";
                return true;
            }

            try
            {
                reply = $"{command.ToUpper()}#{handler.Execute(sender, args.ToArray())}";
                color = "green";
                return true;
            }
            catch (Exception e)
            {
                reply = $"{command.ToUpper()}#An error occured while executing this command.\n{e}";
                color = "red";
                return true;
            }
        }

        public static bool CalculatePermissions(Player player, CommandPermission perm) => perm == CommandPermission.None || player.Permissions.Contains(perm) || player.Permissions.Contains(CommandPermission.All);

        public static IEnumerable<ICommandHandler> GetCommands(IPlugin<IConfig> plugin)
        {
            if (!_commands.ContainsKey(plugin)) 
                _commands.Add(plugin, new List<ICommandHandler>());

            return _commands[plugin];
        }

        public static IEnumerable<ICommandHandler> GetCommands() => _allCommands;
    }
}
