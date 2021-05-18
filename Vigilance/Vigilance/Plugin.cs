using System.Reflection;
using System.Collections.Generic;

using Vigilance.Commands;
using Vigilance.API.Configs;
using Vigilance.API.Plugins;
using Vigilance.Extensions;
using Vigilance.EventSystem;

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

        public void Info(object msg) => Log.Add(Name, msg, LogType.Info);
        public void Error(object msg) => Log.Add(Name, msg, LogType.Error);
        public void Warn(object msg) => Log.Add(Name, msg, LogType.Warn);
        public void Debug(object msg) => Log.Add(Name, msg, LogType.Debug);

        public void AddLog(object msg) => Log.Add(msg);

        public void AddCommand(ICommandHandler commandHandler) => CommandManager.Register((IPlugin<IConfig>)this, commandHandler);
        public void RemoveCommand(string command) => CommandManager.Remove((IPlugin<IConfig>)this, command);
        public void RemoveCommands() => CommandManager.RemoveAll((IPlugin<IConfig>)this);

        public void AddEventHandler(IEventHandler eventHandler) => EventManager.RegisterHandler((IPlugin<IConfig>)this, eventHandler);
        public void RemoveHandler(IEventHandler eventHandler) => EventManager.UnregisterHandler((IPlugin<IConfig>)this, eventHandler);
        public void RemoveHandlers() => EventManager.UnregisterHandlers((IPlugin<IConfig>)this);

        public IEnumerable<ICommandHandler> Commands => CommandManager.GetCommands((IPlugin<IConfig>)this);
    }
}