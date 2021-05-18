using System.Reflection;
using Vigilance.API.Configs;

namespace Vigilance.API.Plugins
{
    public interface IPlugin<out TConfig> where TConfig : IConfig
    {
        string Name { get; }
        string ConfigPrefix { get; }
        TConfig Config { get; }
        Assembly Assembly { get; }

        void OnEnable();
        void OnDisable();
        void OnReload();
    }
}
