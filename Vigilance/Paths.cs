using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System;
using System.Net;
using Vigilance.API;

namespace Vigilance.Utilities
{
	public static class Paths
	{
		public static string SCPSL_Data => Application.dataPath;
		public static string Managed => $"{SCPSL_Data}/Managed";
		public static string Vigilance => $"{SCPSL_Data}/Vigilance";
		public static string VigilanceFile => $"{Managed}/Vigilance.dll";
		public static string HarmonyFile => $"{Managed}/0Harmony.dll";
		public static string Dependencies => $"{Vigilance}/Dependencies";
		public static string Plugins => $"{Vigilance}/Plugins";
		public static string NewtonsoftJson => $"{Managed}/Newtonsoft.Json.dll";
		public static string ConfigPath => $"{ConfigsPath}/{Server.Port}.yml";
		public static string ConfigsPath => $"{Vigilance}/Configs";
		public static string PluginConfigsPath => $"{ConfigsPath}/Plugins/{Server.Port}";
		public static string HarmonyDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v5.5.4/0Harmony.dll";
		public static string NewtonsoftDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v5.5.4/Newtonsoft.Json.dll";

		public static DirectoryInfo Create(string directory) => Directory.CreateDirectory(directory);

		public static void CreateFile(string path)
        {
			if (!File.Exists(path))
            {
				FileStream stream = File.Create(path);
				stream.Close();
            }
        }

		public static void CheckMainConfig()
		{
			if (!Directory.Exists(ConfigsPath)) Directory.CreateDirectory(ConfigsPath);
			if (!File.Exists(ConfigPath)) File.Create(ConfigPath).Close();
		}

		public static void Delete(string directory) => Directory.Delete(directory);

		public static void Check(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Create(directory);
			}
		}

		public static void CheckFile(string path)
        {
			if (!File.Exists(path))
            {
				Log.Add("Paths", $"File {path} does not exist", LogType.Debug);
				CreateFile(path);
            }
        }

		public static string GetPluginConfigPath(Plugin plugin)
        {
			return $"{PluginConfigsPath}/{plugin.Name}.yml";
        }

		public static YamlConfig CheckConfig(string path)
        {
			CheckFile(path);
			return new YamlConfig(path);
        }

		public static void CheckDirectories()
		{
			Check(SCPSL_Data);
			Check(Managed);
			Check(Vigilance);
			Check(Dependencies);
			Check(Plugins);
			Check(ConfigsPath);
			Check(PluginConfigsPath);
			CheckFile(ConfigPath);
		}

		public static void CheckPluginConfig(Dictionary<string, string> configs, string path)
		{
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
		}

		public static void CheckDependencies()
        {
			if (!File.Exists(HarmonyFile))
            {
				Log.Add("Paths", "Downloading 0Harmony.dll", LogType.Info);
				Download(HarmonyDownloadURL, HarmonyFile);
            }

			if (!File.Exists(NewtonsoftJson))
            {
				Log.Add("Paths", "Downloading Newtonsoft.Json.dll", LogType.Info);
				Download(NewtonsoftDownloadURL, NewtonsoftJson);
            }
        }

		public static void DownloadPlugin(string url, string pluginName)
		{
			Log.Add("Paths", $"Downloading \"{pluginName}\" from \"{url}\"", LogType.Debug);
			if (!pluginName.EndsWith(".dll"))
				pluginName += ".dll";
			string path = $"{Plugins}/{pluginName}";
			try
			{
				Download(url, path);
				if (File.Exists(path))
				{
					if (!PluginManager.Assemblies.ContainsKey(path) && !PluginManager.Plugins.ContainsKey(path))
					{
						Assembly assembly = null;
						try
						{
							assembly = Assembly.LoadFrom(path);
						}
						catch (Exception e)
						{
							Log.Add("PluginManager", "An error occured while loading! (1)", LogType.Error);
							Log.Add(e);
						}

						if (assembly != null)
						{
							PluginManager.Assemblies.Add(path, assembly);
							foreach (Type type in assembly.GetTypes())
							{
								if (type.IsAssignableFrom(typeof(Plugin)))
								{
									Plugin plugin = null;
									try
									{
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
											string cfg = GetPluginConfigPath(plugin);
											CheckFile(path);
											plugin.Config = new YamlConfig(cfg);
											plugin.Enable();
											PluginManager.Plugins.Add(path, plugin);
											Log.Add("PluginManager", $"Succesfully loaded {plugin.Name}!", LogType.Info);
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
				}
			}
			catch (Exception e)
			{
				Log.Add("Paths", e);
			}
		}

		public static bool ContainsKey(string[] currentLines, string key)
        {
			foreach (string value in currentLines)
            {
				if (value.ToUpper().StartsWith(key.ToUpper()) || value.ToUpper() == key.ToUpper() || value.StartsWith($"{key}: "))
					return true;
            }
			return false;
        }

		public static void DownloadDependency(string url, string name)
		{
			Log.Add("Paths", $"Downloading \"{name}\" from \"{url}\"", LogType.Debug);
			if (!name.EndsWith(".dll"))
				name += ".dll";
			string path = $"{Dependencies}/{name}";
			Download(url, path);
			if (!PluginManager.Dependencies.ContainsKey(path))
			{
				if (path.EndsWith(".dll"))
				{
					Assembly assembly = null;
					try
					{
						assembly = Assembly.LoadFrom(path);
					}
					catch (Exception e)
					{
						Log.Add("PluginManager", "An error occured while loading dependencies! (5)", LogType.Error);
						Log.Add(e);
					}

					if (assembly != null)
					{
						PluginManager.Dependencies.Add(path, assembly);
						Log.Add("PluginManager", $"Succesfully loaded {assembly.GetName().Name}", LogType.Info);
					}
				}
			}
		}

		public static void Download(string url, string path)
        {
			try
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadFile(url, path);
				}
			}
			catch (Exception e)
            {
				Log.Add("Paths", e);
            }
        }
	}
}
