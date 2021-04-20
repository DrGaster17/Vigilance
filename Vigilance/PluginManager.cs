using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using Version = Vigilance.API.Version;
using Vigilance.Utilities;

namespace Vigilance
{
    public static class PluginManager
    {
        public static Version Version { get; } = new Version(5, 5, 9, "A", false);
        public static List<string> CompatibleVersions = new List<string>() { "10.2.0", "10.2.1", "10.2.2" };
        public static Dictionary<string, Assembly> Assemblies { get; set; }
        public static Dictionary<string, Plugin> Plugins { get; set; }
        public static Dictionary<string, Assembly> Dependencies { get; set; }
        public static YamlConfig Config { get; set; }
        public static HarmonyInstance HarmonyInstance { get; set; }

        public static void Enable()
        {
            try
            {
                Utilities.Utils.InitPluginManager();
                HarmonyInstance = HarmonyInstance.Create("vigilance.patches");
                HarmonyInstance.PatchAll();
                Reload();
                Log.Add("PluginManager", $"Succesfully loaded Vigilance version \"{Version}\"!\nPlugins: {Plugins.Values.Count}\nDependencies: {Dependencies.Values.Count}", LogType.Info);
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An exception occured while enabling!", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }

        public static void Reload()
        {
            try
            {
                Log.Add("Loading dependencies ...", ConsoleColor.Magenta);
                foreach (string file in System.IO.Directory.GetFiles(Paths.Dependencies))
                {
                    if (!Dependencies.ContainsKey(file))
                    {
                        if (file.EndsWith(".dll"))
                        {
                            Assembly assembly = null;
                            try
                            {
                                assembly = Assembly.LoadFrom(file);
                            }
                            catch (Exception e)
                            {
                                Log.Add("PluginManager", "An error occured while loading dependencies! (5)", LogType.Error);
                                Log.Add(e);
                            }

                            if (assembly != null)
                            {
                                Dependencies.Add(file, assembly);
                                Log.Add("PluginManager", $"Succesfully loaded \"{assembly.GetName().Name}\"!", LogType.Info);
                            }
                        }
                    }
                }

                Log.Add("Loading plugins ...", ConsoleColor.Magenta);
                foreach (string file in System.IO.Directory.GetFiles(Paths.Plugins))
                {
                    if (file.EndsWith(".dll"))
                    {
                        bool contains = Assemblies.ContainsKey(file) && Plugins.ContainsKey(file);
                        if (!Assemblies.ContainsKey(file) && !Plugins.ContainsKey(file))
                        {
                            Assembly assembly = null;

                            try
                            {
                                assembly = Assembly.LoadFrom(file);
                            }
                            catch (Exception e)
                            {
                                Log.Add("PluginManager", "An error occured while loading! (1)", LogType.Error);
                                Log.Add(e);
                            }

                            if (assembly != null)
                            {
                                Assemblies.Add(file, assembly);
                                foreach (Type type in assembly.GetTypes())
                                {
                                    if (type.IsSubclassOf(typeof(Plugin)) && type != typeof(Plugin))
                                    {
                                        Plugin plugin = null;
                                        try
                                        {
                                            Log.Add("PluginManager", $"Creating an instance of type \"{type.FullName}\"", LogType.Debug);
                                            plugin = (Plugin)Activator.CreateInstance(type);
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Add("PluginManager", "An error occured while loading! (2)", LogType.Error);
                                            Log.Add(e);
                                        }

                                        if (plugin != null)
                                        {
                                            try
                                            {
                                                string path = Paths.GetPluginConfigPath(plugin);
                                                Paths.CheckFile(path);
                                                plugin.Config = new YamlConfig(path);
                                                plugin.Enable();
                                                Plugins.Add(file, plugin);
                                                Log.Add("PluginManager", $"Succesfully loaded \"{plugin.Name}\"!", LogType.Info);
                                            }
                                            catch (Exception e)
                                            {
                                                Log.Add("PluginManager", $"An error occured while loading {plugin.Name}! (3)", LogType.Error);
                                                Log.Add(e);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Plugins.TryGetValue(file, out Plugin plugin))
                            {
                                try
                                {
                                    if (plugin.Config != null)
                                        plugin.Config.Reload();
                                    plugin.Reload();
                                }
                                catch (Exception e)
                                {
                                    Log.Add("PluginManager", $"An error occured while reloading {plugin.Name}! (4)", LogType.Error);
                                    Log.Add(e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An error occured while loading! (0)", LogType.Error);
                Log.Add(e);
            }
        }
    }
}
