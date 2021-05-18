using System.Collections.Generic;

using Vigilance.Commands;
using Vigilance.API.Configs;

using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.EventSystem;

using Vigilance.Extensions;
using Vigilance.Utilities;
using Vigilance.Plugins.Commands.Components;

using System.ComponentModel;

namespace Vigilance.Plugins.Commands
{
    public class CommandHandler : Plugin<Config>
    {
        public static string ComponentsFilePath = $"{Directories.ConfigsPath}/Components.txt";

        public static List<ICommandHandler> AllCommands = new List<ICommandHandler>
        {
            { new CommandAddSlot() },
            { new CommandAddUnit() },
            { new CommandAdminChat() },
            { new CommandBall() },
            { new CommandClean() },
            { new CommandClearRagdolls() },
            { new CommandCustomItem() },
            { new CommandDropAll() },
            { new CommandDummy() },
            { new CommandExplode() },
            { new CommandFlash() },
            { new CommandForceEnd() },
            { new CommandGhost() },
            { new CommandGiveAll() },
            { new CommandGrenade() },
            { new CommandChangeUnit() },
            { new CommandItemSize() },
            { new CommandList() },
            { new CommandOban() },
            { new CommandPersonalBroadcast() },
            { new CommandPlayers() },
            { new CommandPos() },
            { new CommandRagdoll() },
            { new CommandReload() },
            { new CommandRestart() },
            { new CommandRocket() },
            { new CommandScale() },
            { new CommandTargetGhost() },
            { new CommandTeleport() },
            { new CommandUnban() },
            { new CommandWorkbench() },
            { new CommandSelectDoor() },
            { new CommandRunSpeed() },
            { new CommandWalkSpeed() },
            { new CommandDoorAction() },
            { new CommandSelectPlayer() },
            { new CommandPlayerAction() },
            { new CommandSelectElevator() },
            { new CommandSelectGenerator() },
            { new CommandSelectLocker() },
            { new CommandSelectTesla() },
            { new CommandSelectWindow() },
            { new CommandSelectWorkStation() },
            { new CommandWorkStationAction() },
            { new CommandWindowAction() },
            { new CommandTeslaAction() },
            { new CommandLockerAction() },
            { new CommandGeneratorAction() },
            { new CommandElevatorAction() },
            { new CommandSelectObject() },
            { new CommandObjectAction() },
            { new CommandGrab() },
            { new CommandListComponents() },
            { new CommandSelectPickup() }
        };

        public override void OnEnable()
        {
            RegisterCommands();
        }

        public void RegisterCommands()
        {
            if (!Config.IsEnabled)
            {
                Log.Add("CommandHandler", "Custom commands are disabled in config, skipping ..", LogType.Info);
                return;
            }

            AllCommands.ForEach(x => CommandManager.Register(this, x));

            EventManager.RegisterHandler(this, new EventHandler());
        }
    }

    public class EventHandler : IHandlerDoorInteract, IHandlerRoundRestart, IHandlerElevatorInteract, 
        IHandlerGeneratorClose, IHandlerGeneratorEject, IHandlerGeneratorInsert, IHandlerGeneratorOpen, IHandlerGeneratorUnlock,
        IHandlerLockerInteract, IHandlerTriggerTesla, IHandlerDamageWindow, IHandlerActivateWorkStation, IHandlerDeactivateWorkStation,
        IHandlerPickupItem, IHandlerDroppedItem
    {
        public void OnActivateWorkStation(WorkStationActivateEvent ev)
        {
            if (CommandSelectWorkStation.SelectedStations.ContainsKey(ev.Player))
            {
                CommandSelectWorkStation.SelectedStations[ev.Player] = ev.Workstation;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.station = ev.Workstation;
                    grabber.modified = true;
                }
            }
        }

        public void OnDamageWindow(DamageWindowEvent ev)
        {
            if (CommandSelectWindow.SelectedWindows.ContainsKey(ev.Player))
            {
                CommandSelectWindow.SelectedWindows[ev.Player] = ev.Window;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.window = ev.Window;
                    grabber.modified = true;
                }
            }
        }

        public void OnDeactivateWorkStation(WorkStationDeactivateEvent ev)
        {
            if (CommandSelectWorkStation.SelectedStations.ContainsKey(ev.Player))
            {
                CommandSelectWorkStation.SelectedStations[ev.Player] = ev.Workstation;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.station = ev.Workstation;
                    grabber.modified = true;
                }
            }
        }

        public void OnDoorInteract(DoorInteractEvent ev)
        {
            if (CommandSelectDoor.SelectedDoors.ContainsKey(ev.Player))
            {
                CommandSelectDoor.SelectedDoors[ev.Player] = ev.Door;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.door = ev.Door;
                    grabber.modified = true;
                }
            }
        }

        public void OnElevatorInteract(ElevatorInteractEvent ev)
        {
            if (CommandSelectElevator.SelectedLifts.ContainsKey(ev.Player))
            {
                CommandSelectElevator.SelectedLifts[ev.Player] = ev.Lift.GetElevator();
            }
        }

        public void OnGeneratorClose(GeneratorCloseEvent ev)
        {
            if (CommandSelectGenerator.SelectedGenerators.ContainsKey(ev.Player))
            {
                CommandSelectGenerator.SelectedGenerators[ev.Player] = ev.Generator;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.gen = ev.Generator;
                    grabber.modified = true;
                }
            }
        }

        public void OnGeneratorEject(GeneratorEjectEvent ev)
        {
            if (CommandSelectGenerator.SelectedGenerators.ContainsKey(ev.Player))
            {
                CommandSelectGenerator.SelectedGenerators[ev.Player] = ev.Generator;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.gen = ev.Generator;
                    grabber.modified = true;
                }
            }
        }

        public void OnGeneratorInsert(GeneratorInsertEvent ev)
        {
            if (CommandSelectGenerator.SelectedGenerators.ContainsKey(ev.Player))
            {
                CommandSelectGenerator.SelectedGenerators[ev.Player] = ev.Generator;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.gen = ev.Generator;
                    grabber.modified = true;
                }
            }
        }

        public void OnGeneratorOpen(GeneratorOpenEvent ev)
        {
            if (CommandSelectGenerator.SelectedGenerators.ContainsKey(ev.Player))
            {
                CommandSelectGenerator.SelectedGenerators[ev.Player] = ev.Generator;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.gen = ev.Generator;
                    grabber.modified = true;
                }
            }
        }

        public void OnGeneratorUnlock(GeneratorUnlockEvent ev)
        {
            if (CommandSelectGenerator.SelectedGenerators.ContainsKey(ev.Player))
            {
                CommandSelectGenerator.SelectedGenerators[ev.Player] = ev.Generator;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.gen = ev.Generator;
                    grabber.modified = true;
                }
            }
        }

        public void OnItemDropped(DroppedItemEvent ev)
        {
            if (CommandSelectPickup.Pickups.ContainsKey(ev.Player))
            {
                CommandSelectPickup.Pickups[ev.Player] = ev.Item;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.pickup = ev.Item;
                    grabber.modified = true;
                }
            }
        }

        public void OnLockerInteract(LockerInteractEvent ev)
        {
            if (CommandSelectLocker.SelectedLockers.ContainsKey(ev.Player))
            {
                CommandSelectLocker.SelectedLockers[ev.Player] = ev.Locker;
            }
        }

        public void OnPickupItem(PickupItemEvent ev)
        {
            if (CommandSelectPickup.Pickups.ContainsKey(ev.Player))
            {
                CommandSelectPickup.Pickups[ev.Player] = ev.Item;

                if (!ev.Player.ReferenceHub.TryGetComponent(out Grabber grabber))
                    grabber = ev.Player.AddComponent<Grabber>();

                if (grabber != null)
                {
                    grabber.pickup = ev.Item;
                    grabber.modified = true;
                }
            }
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            CommandSelectDoor.SelectedDoors.Clear();
            CommandSelectPlayer.SelectedPlayers.Clear();
            CommandSelectElevator.SelectedLifts.Clear();
            CommandSelectGenerator.SelectedGenerators.Clear();
            CommandSelectLocker.SelectedLockers.Clear();
            CommandSelectTesla.SelectedGates.Clear();
            CommandSelectWindow.SelectedWindows.Clear();
            CommandSelectWorkStation.SelectedStations.Clear();
            CommandSelectObject.Objects.Clear();
            CommandSelectPickup.Pickups.Clear();

            DestroyGrabbers();
        }

        public void OnTriggerTesla(TriggerTeslaEvent ev)
        {
            if (CommandSelectTesla.SelectedGates.ContainsKey(ev.Player))
            {
                CommandSelectTesla.SelectedGates[ev.Player] = ev.Tesla;
            }
        }

        public void DestroyGrabbers()
        {
            foreach (Grabber grabber in UnityEngine.Object.FindObjectsOfType<Grabber>())
            {
                UnityEngine.Object.Destroy(grabber);
            }
        }
    }

    public class Config : IConfig
    {
        [Description("Enable custom commands")]
        public bool IsEnabled { get; set; }
    }
}
