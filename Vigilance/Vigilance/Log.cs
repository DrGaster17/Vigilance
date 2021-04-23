using System;
using System.Reflection;
using System.Collections.Generic;

namespace Vigilance
{
    public static class Log
    {
        public static Dictionary<LogType, ConsoleColor> Colors { get; } = new Dictionary<LogType, ConsoleColor>()
        {
            { LogType.Debug, ConsoleColor.Cyan },
            { LogType.Error, ConsoleColor.Red },
            { LogType.Info, ConsoleColor.Yellow },
            { LogType.Warn, ConsoleColor.Green },
        };

        public static void Add(object message, LogType type)
        {
            object tag = Assembly.GetCallingAssembly().GetName().Name;

            if (type == LogType.Debug)
            {
                if (PluginManager.Config.ShouldDebug) 
                    Add($"[{type.ToString().ToUpper()}] [{tag}] {message}");

                return;
            }

            Add($"[{type.ToString().ToUpper()}] [{tag}] {message}");
        }

        public static void Add(object tag, object message, LogType type)
        {
            if (type == LogType.Debug)
            {
                if (PluginManager.Config.ShouldDebug) 
                    Add($"[{type.ToString().ToUpper()}] [{tag}] {message}");

                return;
            }

            Add($"[{type.ToString().ToUpper()}] [{tag}] {message}");
        }

        public static void Add(object tag, object message, ConsoleColor color) => Add($"[INFO] [{tag}] {message}");

        public static void Add(object tag, Exception e)
        {
            Add(tag, e.ToString(), LogType.Error);
        }

        public static void Add(Exception e)
        {
            object tag = Assembly.GetCallingAssembly().GetName().Name;
            Add($"[ERROR] [{tag}] {e}");
        }

        public static void Add(Assembly assembly, object message, LogType type)
        {
            if (type == LogType.Debug)
            {
                if (PluginManager.Config.ShouldDebug) 
                    Add($"[{type.ToString().ToUpper()}] [{assembly.GetName().Name}] {message}");

                return;
            }

            Add(assembly.GetName().Name, message, type);
        }

        public static void Info(object message) => Add(message, LogType.Info);
        public static void Info(object tag, object message) => Add(tag, message, LogType.Info);
        public static void Debug(object message) => Add(message, LogType.Debug);
        public static void Debug(object tag, object message) => Add(tag, message, LogType.Debug);
        public static void Warn(object message) => Add(message, LogType.Warn);
        public static void Warn(object tag, object message) => Add(tag, message, LogType.Warn);
        public static void Error(object message) => Add(message, LogType.Error);
        public static void Error(object tag, object message) => Add(tag, message, LogType.Error);
        public static void Add(object log) => ServerConsole.AddLog(log.ToString());
    }

    public enum LogType
    {
        Info,
        Warn,
        Error,
        Debug
    }
}

