using System.IO;
using System.Reflection;
using UnityEngine;
using System;
using System.Net;
using Vigilance.API;
using Vigilance.API.Plugins;
using Vigilance.API.Configs;

namespace Vigilance.External.Utilities
{
	public static class Directories
	{
		public static string SCPSL_Data => Application.dataPath;
		public static string AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		public static string LabPath => GetSlFolder();
		public static string AppDataScp => $"{AppData}/SCP Secret Laboratory";
		public static string Managed => $"{SCPSL_Data}/Managed";

		public static string Vigilance => $"{LabPath}/Vigilance";
		public static string ConfigsPath => $"{Vigilance}/Configs/{Server.Port}";
		public static string Plugins => $"{Vigilance}/Plugins";
		public static string VigilanceFile => $"{Vigilance}/Vigilance.dll";
		public static string VigilanceExtensionsFile => $"{Vigilance}/Vigilance.External.Extensions.dll";
		public static string VigilanceUtilitiesFile => $"{Vigilance}/Vigilance.External.Utilities.dll";
		public static string VigilancePatchesFile => $"{Vigilance}/Vigilance.External.Patching.dll";
		public static string ConfigPath => $"{ConfigsPath}/Config.yml";
		public static string ReservedSlotsFile => $"{AppData}/SCP Secret Laboratory/config/{Server.Port}/UserIDReservedSlots.txt";
		public static string UserIdBans => $"{AppData}//SCP Secret Laboratory/config/{Server.Port}/UserIdBans.txt";
		public static string IpBans => $"{AppData}/SCP Secret Laboratory/config/{Server.Port}/IpBans.txt";
		public static string Whitelist => $"{AppData}/SCP Secret Laboratory/config/{Server.Port}/UserIDWhitelist.txt";
		public static string SlConfigSharing => $"{AppData}/SCP Secret Laboratory/config/{Server.Port}/config_sharing.txt";
		public static string SlGameplayConfig => $"{AppData}/SCP Secret Laboratory/config/{Server.Port}/config_gameplay.txt";
		public static string SlRemoteAdminConfig => $"{AppData}/SCP Secret Laboratory/config/{Server.Port}/config_remoteadmin.txt";
		public static string Dependencies => $"{Vigilance}/Dependencies";
		public static string HarmonyFile => $"{Dependencies}/0Harmony.dll";
		public static string NewtonsoftJson => $"{Dependencies}/Newtonsoft.Json.dll";
		public static string Yaml => $"{Dependencies}/YamlDotNet.dll";

		public static string HarmonyDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v5.5.4/0Harmony.dll";
		public static string NewtonsoftDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v5.5.4/Newtonsoft.Json.dll";
		public static string YamlDownloadURL => $"https://github.com/DrGaster17/Vigilance/releases/download/v5.5.9/YamlDotNet.dll";

		private static string GetSlFolder() => Directory.GetParent(SCPSL_Data).FullName;
		public static DirectoryInfo Create(string directory) => Directory.CreateDirectory(directory);

		public static void Startup()
        {
			CheckDir(SCPSL_Data);
			CheckDir(Managed);
			CheckDir(Vigilance);
			CheckDir(Dependencies);
			CheckDir(Plugins);
			CheckDir(ConfigsPath);
			CheckFile(ConfigPath);

			if (!File.Exists(HarmonyFile))
				Download(HarmonyDownloadURL, HarmonyFile);

			if (!File.Exists(NewtonsoftJson))
				Download(NewtonsoftDownloadURL, NewtonsoftJson);

			if (!File.Exists(Yaml))
				Download(YamlDownloadURL, Yaml);
		}

		public static void CreateFile(string path)
		{
			if (!File.Exists(path))
				File.Create(path).Close();
		}

		public static void CheckDir(string directory)
		{
			if (!Directory.Exists(directory))
				Create(directory);
		}

		public static void CheckFile(string path)
		{
			if (!File.Exists(path))
				CreateFile(path);
		}

		public static YamlConfig GetConfig(string path)
		{
			CheckFile(path);
			return new YamlConfig(path);
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
						Log.Add(e);
					}

					if (assembly != null)
					{
						PluginManager.Dependencies.Add(path, assembly);
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
				Log.Add("Directories", e);
			}
		}
	}
}
