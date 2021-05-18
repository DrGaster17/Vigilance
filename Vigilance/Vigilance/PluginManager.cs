using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using CommandSystem.Commands;

using Vigilance.Utilities;
using Vigilance.Patching;
using Vigilance.API.Configs;
using Vigilance.API.Plugins;
using Vigilance.Configs;

namespace Vigilance
{
    public static class PluginManager
    {
        public const string AssemblyVersion = "6.0.0";

        public static string Description => $"Vigilance is a small, lightweight plugin loader with it's own event system.\nInstalled version: {Version}\n{PluginsToString(Plugins.Values, "Installed plugins")}\n{AssembliesToString(Dependencies.Values, "Installed dependencies")}";
        public static string ServerTracker { get; }

        public static API.Version Version { get; }
        public static Config Config { get; }

        public static List<string> CompatibleVersions { get; }

        public static Dictionary<string, IPlugin<IConfig>> Plugins { get; } 
        public static Dictionary<string, Assembly> Dependencies { get; } 

        static PluginManager()
        {
            Version = new API.Version(6, 0, 0, "", false);
            Config = new Config();

            CompatibleVersions = new List<string>() { "10.2.2" };
            Plugins = new Dictionary<string, IPlugin<IConfig>>();
            Dependencies = new Dictionary<string, Assembly>();

            ServerTracker = $"<color=#00000000><size=1>Vigilance {Version}</size></color>";
        }

        public static void Enable(Assembly[] deps = null)
        {
            try
            {
                PreLoadMessage();

                Reload(deps);

                AfterLoadMessage();
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An exception occured while enabling!", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }

        public static void Disable()
        {
            Dependencies.Clear();

            DisablePlugins();

            Plugins.Clear();

            Patcher.Unpatch();

            Log.Info("PluginManager", $"Thank you for using Vigilance, goodbye!");
        }

        public static void Reload(Assembly[] deps = null)
        {
            try
            {
                Directories.Startup();

                LoadDependencies(deps);

                LoadPlugins();

                ConfigManager.Reload();

                if (!Config.IsEnabled)
                {
                    Log.Add("PluginManager", "Vigilance is disabled by config, disabling ..", LogType.Info);

                    Disable();

                    return;
                }

                Patcher.Patch();

                EnablePlugins();

                CustomNetworkManager.Modded = true;

                BuildInfoCommand.ModDescription = Description;
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An error occured while loading! (0)", LogType.Error);
                Log.Add(e);
            }
        }

        public static void ReloadPatches()
        {
            Patcher.ReloadPatches();
        }

        public static void ReloadPlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins.Values)
            {
                plugin.OnReload();
            }
        }

        public static void DisablePlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins.Values)
            {
                plugin.OnDisable();
            }
        }

        public static void EnablePlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins.Values)
            {
                try
                {
                    plugin.OnEnable();
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", $"An error occured while enabling {plugin.Name}.", LogType.Error);
                    Log.Add("PluginManager", e);
                }
            }
        }

        private static void LoadDependencies(Assembly[] deps = null)
        {
            if (deps != null)
            {
                foreach (Assembly dep in deps)
                {
                    if (!Dependencies.ContainsKey(dep.Location))
                        Dependencies.Add(dep.Location, dep);
                }
            }

            foreach (string path in Directory.GetFiles(Directories.Dependencies, "*.dll"))
            {
                if (!Dependencies.ContainsKey(path))
                {
                    Assembly assembly = LoadAssembly(path);

                    if (assembly == null)
                        continue;

                    Dependencies.Add(path, assembly);
                }
            }
        }

        private static void LoadPlugins()
        {
            bool configReload = false;

            foreach (string pluginPath in Directory.GetFiles(Directories.Plugins, "*.dll"))
            {
                try
                {
                    if (Plugins.TryGetValue(pluginPath, out IPlugin<IConfig> plugin))
                    {
                        if (!configReload)
                        {
                            ConfigManager.Reload();
                            configReload = true;
                        }

                        plugin.OnReload();
                    }
                    else
                    {
                        Assembly assembly = LoadAssembly(pluginPath);

                        if (assembly == null)
                            continue;

                        plugin = LoadPlugin(assembly);

                        if (plugin == null)
                            continue;

                        Plugins.Add(pluginPath, plugin);
                    }
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", $"An error occured while loading {pluginPath}", LogType.Error);
                    Log.Add("PluginManager", e);
                    continue;
                }
            }
        }

        public static Assembly LoadAssembly(string path)
        {
            try
            {
                return Assembly.Load(System.IO.File.ReadAllBytes(path));
            }
            catch (Exception exception)
            {
                Log.Add("PluginManager", $"An error occured while trying to load an assembly at {path}!\n{exception}", LogType.Error);
            }

            return null;
        }

        public static IPlugin<IConfig> LoadPlugin(Assembly assembly)
        {
            try
            {
                foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface))
                {
                    if (!type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(Plugin<>))
                        continue;

                    IPlugin<IConfig> plugin = null;

                    var constructor = type.GetConstructor(Type.EmptyTypes);

                    if (constructor != null)
                    {
                        plugin = constructor.Invoke(null) as IPlugin<IConfig>;
                    }
                    else
                    {
                        var value = Array.Find(type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), property => property.PropertyType == type)?.GetValue(null);

                        if (value != null)
                            plugin = value as IPlugin<IConfig>;
                    }

                    if (plugin == null)
                        continue;

                    return plugin;
                }
            }
            catch (Exception exception)
            {
                Log.Add("PluginManager", $"An error occured while trying to load {assembly.GetName().Name}!\n{exception}", LogType.Error);
            }

            return null;
        }

        public static string AssembliesToString(IEnumerable<Assembly> assemblies, string header)
        {
            int count = assemblies.Count();

            if (count < 1)
                return "No dependencies loaded.";

            string s = $"{header} ({assemblies.Count()}):\n";

            foreach (Assembly assembly in assemblies)
            {
                s += $" - {assembly.GetName().Name} ({assembly.GetName().Version})\n";
            }

            return s;
        }

        public static string PluginsToString(IEnumerable<IPlugin<IConfig>> plugins, string header)
        {
            int count = plugins.Count();

            if (count < 1)
                return "No plugins loaded.";

            string s = $"{header} ({plugins.Count()}):\n";

            foreach (IPlugin<IConfig> plugin in plugins)
            {
                s += $" - {plugin.Name} ({plugin.ConfigPrefix})\n";
            }

            return s;
        }

        private static void PreLoadMessage()
        {
            Log.Info($"Loading version {Version} ..");
            Log.Info($"Welcome!\n{ASCII}");
        }

        private static void AfterLoadMessage()
        {
            Log.Info($"Succesfully loaded version {Version}.");
            Log.Info(PluginsToString(Plugins.Values, "Installed plugins"));
            Log.Info(AssembliesToString(Dependencies.Values, "Installed dependencies"));
            Log.Info($"Thank you for using Vigilance!");

            Holidays();
        }

        private static void Holidays()
        {
            if (ClutterSpawner.IsHolidayActive(global::Holidays.AprilFools))
                Log.Info("Happy April Fools!");

            if (ClutterSpawner.IsHolidayActive(global::Holidays.Christmas))
                Log.Info("Merry Christmas!");

            if (ClutterSpawner.IsHolidayActive(global::Holidays.Halloween))
                Log.Info("Happy Halloween!");
        }

        public const string ASCII = "" +
            " _   _ _       _ _\n" +
            "| | | (_)     (_) |\n" +
            "| | | |_  __ _ _| | __ _ _ __   ___ ___\n" +
            "| | | | |/ _` | | |/ _` | '_ \\ / __/ _ \\\n" +
            "\\ \\_/ / | (_| | | | (_| | | | | (_|  __/\n" +
            " \\___/|_|\\__, |_|_|\\__,_|_| |_|\\___\\___|\n" +
            "          __/ |\n" +
            "         |___/\n";
    }
}
