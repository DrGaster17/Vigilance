using System;
using System.Collections.Generic;
using Harmony;

namespace Vigilance.Patching
{
    public static class Patcher
    {
        private static HarmonyInstance _instance;

        public static readonly List<string> DisabledPatches = new List<string>();
        public static readonly API.Version RequiredVersion = new API.Version(6, 0, 0, "", false);

        public static void Patch()
        {
            try
            {
                if (PluginManager.Version.GetHashCode() != RequiredVersion.GetHashCode() || (PluginManager.Version.FullName != RequiredVersion.FullName))
                {
                    Vigilance.Log.Add(typeof(Patcher).FullName, $"This version ({RequiredVersion}) is not compatible with Vigilance version {PluginManager.Version}! Minimum required version: {RequiredVersion}", LogType.Error);
                    return;
                }

                if (_instance == null)
                    _instance = HarmonyInstance.Create("vigilance.patching.patcher");

                _instance.PatchAll();
            }
            catch (Exception e)
            {
                Vigilance.Log.Add(typeof(Patcher).FullName, $"An error occured while patching!\n{e}", LogType.Error);
            }
        }

        public static void Unpatch()
        {
            _instance.UnpatchAll();
        }

        public static void ReloadPatches()
        {
            Unpatch();

            Patch();
        }

        public static void EnablePatch(string fullName)
        {
            if (!DisabledPatches.Remove(fullName))
                return;

            ReloadPatches();
        }

        public static void EnablePatch(Type declaringType, string methodName)
        {
            if (!DisabledPatches.Remove($"{declaringType.FullName}.{methodName}"))
                return;

            ReloadPatches();
        }

        public static void DisablePatch(string fullName)
        {
            DisabledPatches.Add(fullName);

            ReloadPatches();
        }

        public static void DisablePatch(Type declaringType, string methodName)
        {
            DisabledPatches.Add($"{declaringType.FullName}.{methodName}");

            ReloadPatches();
        }

        public static void Log(Type patch, object message)
        {
            if (!PluginManager.Config.PatcherEventDebug)
                return;

            string name = patch.FullName.Replace('_', '.');

            Vigilance.Log.Add(name, message, IsException(message) ? ConsoleColor.Red : ConsoleColor.Green);
        }

        public static void LogWarn(Type patch, object message)
        {
            if (!PluginManager.Config.PatcherEventDebug)
                return;

            string name = patch.FullName.Replace('_', '.');

            Vigilance.Log.Add(name, message, ConsoleColor.Yellow);
        }

        public static bool IsException(object message) => (message is Exception e && e != null) || message.ToString().Contains("Exception");
    }
}
