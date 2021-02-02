using System;
using System.Reflection;
using Vigilance.Extensions;

namespace Vigilance
{
    public static class Log
    {
        public static void Add(object message, LogType type)
        {
            object tag = Assembly.GetCallingAssembly().GetName().Name;
            if (type == LogType.Debug)
            {
                if (ConfigManager.ShouldDebug) Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", type.GetColor());
				return;
            }
            Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", type.GetColor());
        }

        public static void Add(object tag, object message, LogType type)
        {
            if (type == LogType.Debug)
            {
                if (ConfigManager.ShouldDebug) Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", type.GetColor());
				return;
            }
            Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", type.GetColor());
        }

        public static void Add(object tag, object message, ConsoleColor color) => Add($"[{tag}]: {message}", color);

        public static void Add(object tag, Exception e)
        {
            Add(tag, e.ToString(), LogType.Error);
        }

        public static void Add(Exception e)
        {
            object tag = Assembly.GetCallingAssembly().GetName().Name;
            Add($"[ERROR] [{tag}]: {e}", ConsoleColor.DarkRed);
        }

        public static void Add(Assembly assembly, object message, LogType type)
        {
            if (type == LogType.Debug)
            {
                if (ConfigManager.ShouldDebug) Add($"[{type.ToString().ToUpper()}] [{assembly.GetName().Name}]: {message}", type.GetColor());
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
        public static void Add(object log, ConsoleColor color = ConsoleColor.Magenta) => ServerConsole.AddLog(log.ToString(), color);
    }

    public enum LogType
    {
        Info,
        Warn,
        Error,
        Debug
    }
}
