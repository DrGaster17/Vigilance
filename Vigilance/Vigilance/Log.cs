using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Vigilance
{
    public static class Log
    {
        private static Dictionary<Stream, bool> _addOutputs;

        static Log()
        {
            _addOutputs = new Dictionary<Stream, bool>();
        }

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

        public static void AddOutput(Stream output, bool serverOutput) => _addOutputs.Add(output, serverOutput);
        public static bool RemoveOutput(Stream output) => _addOutputs.Remove(output);

        public static void Info(object message) => Add(message, LogType.Info);
        public static void Info(object tag, object message) => Add(tag, message, LogType.Info);

        public static void Debug(object message) => Add(message, LogType.Debug);
        public static void Debug(object tag, object message) => Add(tag, message, LogType.Debug);

        public static void Warn(object message) => Add(message, LogType.Warn);
        public static void Warn(object tag, object message) => Add(tag, message, LogType.Warn);

        public static void Error(object message) => Add(message, LogType.Error);
        public static void Error(object tag, object message) => Add(tag, message, LogType.Error);

        public static void Debug(object message, bool debug)
        {
            if (!debug)
                return;

            Debug(message);
        }

        public static void Debug(object tag, object message, bool debug)
        {
            if (!debug)
                return;

            Debug(tag, message);
        }

        public static void Add(object log)
        {
            if (_addOutputs.Count > 1)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(log.ToString());

                foreach (KeyValuePair<Stream, bool> pair in _addOutputs)
                {
                    pair.Key.Write(bytes, 0, bytes.Length);
                }
            }

            ServerConsole.AddLog(log.ToString());
        }

        public static void WriteServerMessageToOutputs(object message)
        {
            if (_addOutputs.Count > 1)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message.ToString());

                foreach (KeyValuePair<Stream, bool> pair in _addOutputs)
                {
                    if (!pair.Value)
                        continue;

                    pair.Key.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }

    public enum LogType
    {
        Info,
        Warn,
        Error,
        Debug
    }
}

