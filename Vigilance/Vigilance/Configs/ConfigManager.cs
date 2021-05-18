using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Vigilance.API.Plugins;
using Vigilance.API.Configs;
using Vigilance.API.Integrations;

using Vigilance.Utilities;
using Vigilance.Extensions;

namespace Vigilance.Configs
{
    public static class ConfigManager
    {
        public static ISerializer Serializer { get; } = new SerializerBuilder().WithTypeInspector(inner => new CommentGatheringTypeInspector(inner)).WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor)).WithNamingConvention(UnderscoredNamingConvention.Instance).IgnoreFields().Build();
        public static IDeserializer Deserializer { get; } = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>()).IgnoreFields().IgnoreUnmatchedProperties().Build();

        public static Dictionary<string, IConfig> Load(string rawConfigs)
        {
            try
            {
                rawConfigs = Regex.Replace(rawConfigs, @"\ !.*", string.Empty).Replace("!Dictionary[string,IConfig]", string.Empty);
                Dictionary<string, object> rawDeserializedConfigs = Deserializer.Deserialize<Dictionary<string, object>>(rawConfigs) ?? new Dictionary<string, object>();
                Dictionary<string, IConfig> deserializedConfigs = new Dictionary<string, IConfig>();

                if (!rawDeserializedConfigs.TryGetValue("vigilance_config", out object rawDeserializedConfig))
                {
                    deserializedConfigs.Add("vigilance_config", PluginManager.Config);
                }
                else
                {
                    deserializedConfigs.Add("vigilance_config", Deserializer.Deserialize<Config>(Serializer.Serialize(rawDeserializedConfig)));
                    PluginManager.Config.CopyProperties(deserializedConfigs["vigilance_config"]);
                    ScpSlIntegration.Integration.MaxTimeout = PluginManager.Config.MaxAllowedTimeout;
                }

                foreach (IPlugin<IConfig> plugin in PluginManager.Plugins.Values)
                {
                    if (!rawDeserializedConfigs.TryGetValue(plugin.ConfigPrefix, out rawDeserializedConfig))
                    {
                        deserializedConfigs.Add(plugin.ConfigPrefix, plugin.Config);
                    }
                    else
                    {
                        try
                        {
                            deserializedConfigs.Add(plugin.ConfigPrefix, (IConfig)Deserializer.Deserialize(Serializer.Serialize(rawDeserializedConfig), plugin.Config.GetType()));
                            plugin.Config.CopyProperties(deserializedConfigs[plugin.ConfigPrefix]);
                        }
                        catch (YamlException)
                        {
                            deserializedConfigs.Add(plugin.ConfigPrefix, plugin.Config);
                        }
                    }
                }

                return deserializedConfigs;
            }
            catch (Exception e)
            {
                Log.Add("ConfigManager", $"An error occured while reloading configs!\n{e}", LogType.Error);
                return null;
            }
        }

        public static bool Reload() => Save(Load(Read()));
        public static bool Save(string configs)
        {
            try
            {
                System.IO.File.WriteAllText(Directories.ConfigPath, configs ?? string.Empty);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Save(Dictionary<string, IConfig> configs)
        {
            try
            {
                if (configs == null || configs.Count == 0)
                    return false;
                return Save(Serializer.Serialize(configs));
            }
            catch (YamlException yamlException)
            {
                Log.Add(yamlException);
                return false;
            }
        }

        public static string Read()
        {
            try
            {
                if (System.IO.File.Exists(Directories.ConfigPath))
                    return System.IO.File.ReadAllText(Directories.ConfigPath);
            }
            catch (Exception exception)
            {
                Log.Add(exception);
            }

            return string.Empty;
        }

        public static bool Clear() => Save(string.Empty);
    }
}