using System.Reflection;
using Vigilance.API.Configs;
using Vigilance.API.Plugins;
using Vigilance.External.Extensions;

namespace Vigilance
{
    public abstract class Plugin<TConfig> : IPlugin<TConfig> where TConfig : IConfig, new()
    {
        public Plugin()
        {
            Name = Assembly.GetName().Name;
            ConfigPrefix = Name.ToSnakeCase();
        }

        public Assembly Assembly { get; } = Assembly.GetCallingAssembly();

        public virtual string Name { get; }
        public virtual string ConfigPrefix { get; }

        public TConfig Config { get; } = new TConfig();

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnReload() { }

        public int CompareTo(IPlugin<IConfig> other) => 0;
    }
}
