using System;
using System.Linq;
using System.Collections.Generic;

// For the plugin class - Vigilance.dll
using Vigilance;

// For events - Vigilance.dll
using Vigilance.EventSystem.Events;

// For event handlers - Vigilance.dll
using Vigilance.EventSystem.EventHandlers;

// For plugin extensions like adding commands or event handlers - Vigilance.External.Extensions.dll
using Vigilance.External.Extensions;

// For commands - Vigilance.dll
using Vigilance.Commands;

// For enums - Vigilance.dll
using Vigilance.API.Enums;

// For the player class - Vigilance.dll
using Vigilance.API;

// For coroutines - Assembly-CSharp-firstpass.dll
using MEC;

namespace Vigilance.External.Example
{
    public class ExamplePlugin : Plugin<ExampleConfig>
    {
        // Defaults to the Assembly name, change if neccesary
        public override string Name => base.Name;

        // Defaults to the Asembly name (in snake case), change if neccesary
        // The prefix your configs will generate under

        // prefix:
        //    config_key: value
        public override string ConfigPrefix => base.ConfigPrefix;

        // Not needed, but recommened if you need
        // to access some instance-only functions
        // or possibly your config
        public static ExamplePlugin Singleton;

        // private EventHandler _handler;
        // private TestCommand  _cmd;

        // Called when the plugin is enabled
        public override void OnEnable()
        {
            try
            {
                // Make sure to set the singleton 
                // (if you have one)
                Singleton = this;

                // Add your event handler
                // If you want to add and remove a single instance of your
                // event handler make sure to save it in a variable
                // _handler = new EventHandlers();
                // this.AddEventHandler(_handler);
                this.AddEventHandler(new EventHandler());

                // Add your command handler
                // If you want to add and remove a single instance of your
                // command handler make sure to save it in a variable
                // _cmd = new TestCommand();
                // this.AddCommand(_cmd);
                this.AddCommand(new TestCommand());
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        // Called when the plugin is being disabled
        // It is recommended to remove all of your event handlers and commands
        // and kill all of your coroutines in this function
        public override void OnDisable()
        {
            // Remove all of your event handlers
            this.RemoveHandlers();

            // Remove all of your command handlers
            this.RemoveCommands();

            // Kill all coroutine (if it exists)
            if (EventHandler.Handle.HasValue)
                Timing.KillCoroutines(EventHandler.Handle.Value);
        }

        // Called when the plugin is reloaded
        public override void OnReload()
        {
            // Adds a log to the server console with the "Reloaded." message.
            this.Info("Reloaded.");
        }
    }

    public class EventHandler : IHandlerWaitingForPlayers, IHandlerPlayerSpawn, IHandlerRoundStart
    {
        // A variable containg our coroutine
        public static CoroutineHandle? Handle;

        // This method executes when a player spawns
        // IHandlerPlayerSpawn
        public void OnSpawn(PlayerSpawnEvent ev)
        {
            // Broadcast to the spawned player
            ev.Player.Broadcast($"You spawned as a {ev.Role} with {ev.Health} health and {ev.Items.Count()} items.", 10, true);
        }

        // This method executes when the servers starts waiting for players
        // IHandlerWaitingForPlayers
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            // Adds a log to the server console with the "Waiting for players!" message.
            ExamplePlugin.Singleton.Info("Waiting for players!");
        }

        // This method executes when the round start
        // IHandlerRoundStart
        public void OnRoundStart(RoundStartEvent ev)
        {
            // Start our coroutine
            Handle = Timing.RunCoroutine(ExampleCoroutine());
        }

        // This is a example coroutine.
        // What is a coroutine? Well ..
        // Coroutines are timed methods, that support waiting periods of time before continuing execution, 
        // without interrupting/sleeping the main game thread. MEC coroutines are safe to use with Unity, 
        // unlike traditional threading.
        // You need to reference Assembly-CSharp-firstpass.dll to use them.
        public IEnumerator<float> ExampleCoroutine()
        {
            // Count how many times our method has executed
            int index = 0;

            // A for (;;) will create a infinite loop until you stop it with a yield break;
            for (; ; )
            {
                // Add 1 to our index
                index++;

                // Spawn an item at all players on the server
                PlayersList.List.ForEach(x => Map.SpawnItem(ItemType.GrenadeFrag, x.Position, x.Rotations));

                // Check if index is higher than or equal to 45, if it is then stop this coroutine.
                if (index >= 45)
                {
                    // Spawn a grenade that will trigger all other 45 grenades ... what fun.
                    PlayersList.List.ForEach(x => x.SpawnGrenade(GrenadeType.Frag));

                    // Kill our coroutine
                    yield break;
                }
            }
        }
    }

    public class TestCommand : ICommandHandler
    {
        // Command name
        public string Command => "test";

        // A string array of command aliases
        public string[] Aliases => new string[] { "t", "tt" };

        // Where will this command be executed
        // For this example we will be using a RemoteAdmin command
        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin, CommandType.ServerConsole };

        // Custom command permissions
        public CommandPermission Permission => CommandPermission.None;

        // This method executes when someone calls this command
        // sender -> the person that executed it
        // args -> command arguments (without the command itself)
        public string Execute(Player sender, string[] args)
        {
            // Check if the sender wrote all required arguments
            // The command itself does not count as an argument
            if (args.Length < 1)
            {
                // Returns a command response
                return $"Missing arguments!\nUsage: test <text>";
            }
            else
            {
                // Combines an array into a string
                string text = args.Combine();

                // Check if the amount of configured items is more than 0
                if (ExamplePlugin.Singleton.Config.Items.Count > 0)
                {
                    // Give all of the items in config to the sender
                    ExamplePlugin.Singleton.Config.Items.ForEach(x => sender.AddItem(x));
                }

                // Returns a command response
                return text;
            }
        }
    }
}
