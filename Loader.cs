using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Reflection;

namespace Vigilance.API.Integrations
{
    public abstract class Integration
    {
		public string SCPSL_Data => Application.dataPath;
		public string AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		public string LabPath => GetSlFolder();
		public string AppDataScp => $"{AppData}/SCP Secret Laboratory";
		public string Managed => $"{SCPSL_Data}/Managed";

		public string Vigilance => $"{LabPath}/Vigilance";
		public string ConfigsPath => $"{LabPath}/Configs/{Port}";

		public string VigilanceFile => $"{Vigilance}/Vigilance.dll";
		public string VigilanceExtensionsFile => $"{Vigilance}/Vigilance.External.Extensions.dll";
		public string VigilanceUtilitiesFile => $"{Vigilance}/Vigilance.External.Utilities.dll";
		public string VigilancePatchesFile => $"{Vigilance}/Vigilance.External.Patching.dll";
		public string VigilanceDiscordFile => $"{Vigilance}/Vigilance.External.Discord.dll";

		public string Dependencies => $"{Vigilance}/Dependencies";
		public string HarmonyFile => $"{Dependencies}/0Harmony.dll";
		public string NewtonsoftJson => $"{Dependencies}/Newtonsoft.Json.dll";
		public string Yaml => $"{Dependencies}/YamlDotNet.dll";

		public string GetSlFolder() => Directory.GetParent(SCPSL_Data).FullName;

		public int Port => ServerStatic.ServerPortSet ? ServerStatic.ServerPort : 7777;

        public List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

        public bool LoadedSuccesfully { get; set; }

        public abstract void Load();
        public abstract void Unload();
        public abstract void Reload();

        public bool TryLoad(out string error)
        {
			Log($"Server Port: {Port}", false, true);
			Log($"Vigilance Path: {VigilanceFile}", false, true);
			Log($"Vigilance Extensions Path: {VigilanceExtensionsFile}", false, true);
			Log($"Vigilance Utilities Path: {VigilanceUtilitiesFile}", false, true);
			Log($"Vigilance Patches Path: {VigilancePatchesFile}", false, true);
			Log($"Vigilance Discord Path: {VigilanceDiscordFile}", false, true);

            if (!File.Exists(VigilanceUtilitiesFile))
            {
                Log($"Could not find \"Vigilance.External.Utilities.dll\"! Skipping the loading process ..", true, false);
                error = "Vigilance.External.Utilities.dll not found.";
                return false;
            }

            Assembly utilities = Assembly.LoadFrom(VigilanceUtilitiesFile);

            if (utilities != null)
            {
                Log($"Succesfully loaded Vigilance.External.Utilities!");
                LoadedAssemblies.Add(utilities);
            }
            else
            {
                Log($"Vigilance.External.Utilites could not be loaded, skipping the loading process ..", true);
                error = "Vigilance.External.Utilites could not be loaded.";
                return false;
            }

            try
            {
                utilities.GetType("Vigilance.External.Utilities.Directories")?.GetMethod("Startup")?.Invoke(null, null);
            }
            catch (Exception e)
            {
                Log("An error occured while executing the \"Vigilance.External.Utilities.Directories.Startup\" method.", true, false);
                Log(e, true);
                error = e.ToString();
                return false;
            }

            if (!File.Exists(VigilancePatchesFile))
            {
                Log($"Could not find \"Vigilance.External.Patching.dll\"! Skipping the loading process ..", true, false);
                error = "Vigilance.External.Patching.dll not found.";
                return false;
            }

            if (!File.Exists(VigilanceExtensionsFile))
            {
                Log($"Could not find \"Vigilance.External.Extensions.dll\"! Skipping the loading process ..", true, false);
                error = "Vigilance.External.Extensions.dll not found.";
                return false;
            }

            if (!File.Exists(VigilanceFile))
            {
                Log($"Could not find \"Vigilance.dll\"! Skipping the loading process ..", true, false);
                error = "Vigilance.dll not found.";
                return false;
            }

            if (!File.Exists(HarmonyFile))
            {
                Log($"Could not find \"0Harmony.dll\"! Skipping the loading process ..", true, false);
                error = "0Harmony.dll not found.";
                return false;
            }

            if (!File.Exists(NewtonsoftJson))
            {
                Log($"Could not find \"Newtonsoft.Json.dll\"! Skipping the loading process ..", true, false);
                error = "Newtonsoft.Json.dll not found.";
                return false;
            }

            if (!File.Exists(Yaml))
            {
                Log($"Could not find \"Vigilance.External.Patching.dll\"! Skipping the loading process ..", true, false);
                error = "Vigilance.External.Patching.dll not found.";
                return false;
            }

            Assembly patcher = Assembly.LoadFrom(VigilancePatchesFile);
            Assembly extensions = Assembly.LoadFrom(VigilanceExtensionsFile);
            Assembly vigilance = Assembly.LoadFrom(VigilanceFile);
            Assembly harmonyAssembly = Assembly.LoadFrom(HarmonyFile);
            Assembly newtonsoft = Assembly.LoadFrom(NewtonsoftJson);
            Assembly yamlDotNet = Assembly.LoadFrom(Yaml);
			
			Assembly discord = null;
			
			if (File.Exists(VigilanceDiscordFile))
				discord = Assembly.LoadFrom(VigilanceDiscordFile);

            if (patcher != null)
            {
                Log($"Succesfully loaded Vigilance.External.Patching!");
                LoadedAssemblies.Add(patcher);
            }
            else
            {
                Log($"Vigilance.External.Patching could not be loaded, skipping the loading process ..", true);
                error = "Vigilance.External.Patching could not be loaded.";
                return false;
            }

            if (extensions != null)
            {
                Log($"Succesfully loaded Vigilance.External.Extensions!");
                LoadedAssemblies.Add(extensions);
            }
            else
            {
                Log($"Vigilance.External.Extensions could not be loaded, skipping the loading process ..", true);
                error = "Vigilance.External.Extensions could not be loaded.";
                return false;
            }
			
			if (discord != null)
			{
				Log($"Succesfully loaded Vigilance.External.Discord!");
				LoadedAssemblies.Add(discord);
			}

            if (vigilance != null)
            {
                Log($"Succesfully loaded Vigilance!");
            }
            else
            {
                Log($"Vigilance could not be loaded, skipping the loading process ..", true);
                error = "Vigilance could not be loaded.";
                return false;
            }

            if (harmonyAssembly != null)
            {
                Log($"Succesfully loaded 0Harmony!");
                LoadedAssemblies.Add(harmonyAssembly);
            }
            else
            {
                Log($"0Harmony could not be loaded, skipping the loading process ..", true);
                error = "0Harmony could not be loaded.";
                return false;
            }

            if (newtonsoft != null)
            {
                Log($"Succesfully loaded Newtonsoft.Json!");
                LoadedAssemblies.Add(newtonsoft);
            }
            else
            {
                Log($"Newtonsoft.Json could not be loaded, skipping the loading process ..", true);
                error = "Newtonsoft.Json could not be loaded.";
                return false;
            }

            if (yamlDotNet != null)
            {
                Log($"Succesfully loaded YamlDotNet!");
                LoadedAssemblies.Add(yamlDotNet);
            }
            else
            {
                Log($"YamlDotNet could not be loaded, skipping the loading process ..", true);
                error = "YamlDotNet could not be loaded.";
                return false;
            }

            try
            {
                vigilance.GetType("Vigilance.PluginManager")?.GetMethod("Enable").Invoke(null, new object[] { LoadedAssemblies.ToArray() });
            }
            catch (Exception e)
            {
                Log("An error occured while executing the \"Vigilance.PluginManager.Enable\" method.", true, false);
                Log(e, true);
                error = e.ToString();
                return false;
            }

            error = "No Error.";
            LoadedSuccesfully = true;
            return true;
        }

        public void Log(object msg, bool isError = false, bool isDebug = false)
        {
            if (isDebug)
            {
                ServerConsole.AddLog($"[DEBUG] [LOADER] {msg}");
                return;
            }

            if (isError)
            {
                ServerConsole.AddLog($"[ERROR] [LOADER] {msg}");
            }
            else
            {
                ServerConsole.AddLog($"[INFO] [LOADER] {msg}");
            }
        }
    }
	
	public class ScpSlIntegration: Integration
	{
		public static ScpSlIntegration Integration;
		
		public ScpSlIntegration() => Integration = this;
		
		public override void Load()
		{
			if (!TryLoad(out _))
				return;
		}
		
		public override void Unload()
		{
			if (!LoadedSuccesfully)
				return;
			
			LoadedAssemblies.FirstOrDefault(x => x.GetName().Name == "Vigilance").GetType("Vigilance.PluginManager").GetMethod("DisablePlugins").Invoke(null, null);
		}
		
		public override void Reload()
		{
			if (!LoadedSuccesfully)
				return;
			
			LoadedAssemblies.FirstOrDefault(x => x.GetName().Name == "Vigilance").GetType("Vigilance.PluginManager").GetMethod("Reload").Invoke(null, null);	
		}
	}
}