using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using CommandSystem.Commands;
using Vigilance.External.Utilities;
using Vigilance.External.Patching;
using Vigilance.API.Configs;
using Vigilance.API.Plugins;
using Vigilance.Configs;

namespace Vigilance
{
    public static class PluginManager
    {
        public const string AssemblyVersion = "6.0.0";

        public static API.Version Version { get; }
        public static Config Config { get; }

        public static List<string> CompatibleVersions { get; }
        public static Dictionary<Assembly, string> Locations { get; } 
        public static Dictionary<IPlugin<IConfig>, Assembly> PluginAssemblies { get; } 
        public static Dictionary<string, Assembly> Assemblies { get; } 
        public static Dictionary<string, IPlugin<IConfig>> Plugins { get; } 
        public static Dictionary<string, Assembly> Dependencies { get; } 

        static PluginManager()
        {
            Version = new API.Version(6, 0, 0, "RC", false);
            Config = new Config();

            CompatibleVersions = new List<string>() { "10.2.0", "10.2.1", "10.2.2" };
            Locations = new Dictionary<Assembly, string>();
            PluginAssemblies = new Dictionary<IPlugin<IConfig>, Assembly>();
            Assemblies = new Dictionary<string, Assembly>();
            Plugins = new Dictionary<string, IPlugin<IConfig>>();
            Dependencies = new Dictionary<string, Assembly>();
        }

        public static void Enable(Assembly[] deps = null)
        {
            try
            {
                Reload(deps);

                Log.Add("PluginManager", $"Vigilance {Version} loaded succesfully.\nLoaded {Dependencies.Count} dependencies.\nLoaded {Plugins.Count} plugin(s).", ConsoleColor.Cyan);
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An exception occured while enabling!", LogType.Error);
                Log.Add("PluginManager", e);
            }
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
                    Log.Add("PluginManager", "Vigilance is disabled by config, unloading all plugins ..", LogType.Info);

                    Locations.Clear();
                    PluginAssemblies.Clear();
                    Assemblies.Clear();
                    Plugins.Clear();
                    Dependencies.Clear();

                    return;
                }

                Patcher.Patch();

                EnablePlugins();

                CustomNetworkManager.Modded = Config.MarkAsModded;
                BuildInfoCommand.ModDescription = $"Vigilance {Version} - a simple plugin loader for SCP: Secret Laboratory\nLoaded {Plugins.Count} plugin(s)\nLoaded {Dependencies.Count} dependencies";
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
                if (!plugin.Config.IsEnabled)
                    continue;

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

                        Locations.Add(assembly, pluginPath);
                        Plugins.Add(pluginPath, plugin);
                        PluginAssemblies.Add(plugin, assembly);
                        Assemblies.Add(pluginPath, assembly);
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
                return Assembly.Load(File.ReadAllBytes(path));
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
    }
}
