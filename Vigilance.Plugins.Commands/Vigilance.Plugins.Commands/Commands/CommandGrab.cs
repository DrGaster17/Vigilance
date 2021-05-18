using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.Extensions;
using Vigilance.Plugins.Commands.Components;

using Interactables;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.Plugins.Commands
{
    public class CommandGrab : ICommandHandler
    {
        public string Command => "grab";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public string Execute(Player sender, string[] args)
        {
            if (!sender.ReferenceHub.TryGetComponent(out Grabber grab))
                grab = sender.AddComponent<Grabber>();

            if (grab == null)
            {
                return "Cannot initialize the Grabber component.";
            }

            if (grab.modified && !grab.allow)
            {
                grab.allow = true;
                return "Allowed grabbing.";
            }

            if (grab.isGrabbing)
            {
                grab.Stop();

                return "Stopped.";
            }

            GameObject obj = null;

            if (!CommandSelectObject.Objects.TryGetValue(sender, out obj))
                obj = sender.LookingAt;
            
            if (obj == null)
            {
                return "You have to look at something you are able to grab (Player, Item, Window, Door, WorkStation) first. Make sure you are close enough to the object as there might be another Collider or some NW's bullshit between it and you.";
            }

            if (args.Length > 0)
            {
                if (args[0] == "allow")
                {
                    grab.allow = true;
                    return "Enabled grabbing.";
                }

                if (args[0] == "null")
                {
                    grab.Stop();
                    return "Nulled all objects.";
                }

                if (args[0] == "player")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    // shut up VS
                    CharacterClassManager ccm = null;

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.Player) || Physics.Raycast(ray, out hit, float.MaxValue, (int)LayerType.Player))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out ReferenceHub hub) || hit.transform.gameObject.TryGetComponent(out hub) 
                            || hit.transform.parent.gameObject.TryGetComponent(out hub) || hit.collider.gameObject.TryGetComponent(out ccm) 
                            || hit.transform.TryGetComponent(out ccm) || hit.transform.parent.gameObject.TryGetComponent(out ccm) || hit.rigidbody.TryGetComponent(out hub) || hit.rigidbody.TryGetComponent(out ccm))
                        {
                            if (hub != null)
                            {
                                Player player = PlayersList.GetPlayer(hub);

                                grab.Grab(player);
                                return $"Grabbed {player.Nick}";
                            }
                            else
                            {
                                if (ccm != null)
                                {
                                    Player player = PlayersList.GetPlayer(ccm._hub);

                                    grab.Grab(player);
                                    return $"Grabbed {player.Nick}";
                                }
                                else
                                {
                                    Log.Error("Grabber", "Raycast has succesfully hit a Player Layer, but the Player Hit does not have a CharacterClassManager component.");
                                    grab.Grab(hit.collider.gameObject);
                                    return "Raycast has succesfully hit a Player Layer, but the Player Hit does not have a CharacterClassManager component.";
                                }
                            }
                        }
                        else
                        {
                            Log.Error("Grabber", "Raycast has succesfully hit a Player Layer, but the Player Hit does not have a ReferenceHub component.");
                            grab.Grab(hit.collider.gameObject);
                            return "Raycast has succesfully hit a Player Layer, but the Player Hit does not have a ReferenceHub component.";
                        }
                    }
                }

                if (args[0] == "pickup")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.Pickup))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out Pickup pickup) || hit.transform.gameObject.TryGetComponent(out pickup) 
                            || hit.transform.parent.gameObject.TryGetComponent(out pickup) || hit.rigidbody.TryGetComponent(out pickup))
                        {
                            grab.Grab(pickup);
                            return $"Grabbed {pickup.itemId}";
                        }
                        else
                        {
                            Log.Error("Grabber", "Raycast has succesfully hit a Pickup Layer, but the Pickup Hit does not have a Pickup component.");
                            grab.Grab(hit.collider.gameObject);
                            return "Raycast has succesfully hit a Pickup Layer, but the Pickup Hit does not have a Pickup component.";
                        }
                    }
                }

                if (args[0] == "door")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    InteractableCollider coll = null;

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.Door) || Physics.Raycast(ray, out hit, float.MaxValue, (int)LayerType.DestoyedDoor))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out DoorVariant door) || hit.transform.gameObject.TryGetComponent(out door) 
                            || hit.collider.gameObject.transform.parent.TryGetComponent(out door) || hit.rigidbody.TryGetComponent(out door)
                            || hit.collider.gameObject.TryGetComponent(out coll))
                        {
                            if (door != null)
                            {
                                API.Door d = door.GetDoor();

                                grab.Grab(d);
                                return $"Grabbed a door of type {d.Type}";
                            }
                            else
                            {
                                if (coll != null)
                                {
                                    door = coll.Target.GetComponent<DoorVariant>() ?? coll.Target as DoorVariant;

                                    if (door != null)
                                    {
                                        API.Door d = door.GetDoor();

                                        grab.Grab(d);
                                        return $"Grabbed a door of type {d.Type}";
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Error("Grabber", "Raycast has succesfully hit a Door Layer, but the Door does not have a DoorVariant component.");
                            grab.Grab(hit.collider.gameObject);
                            return "Raycast has succesfully hit a Door Layer, but the Door does not have a DoorVariant component.";
                        }
                    }
                }

                if (args[0] == "blood")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.BloodAndDecals))
                    {
                        grab.Grab(hit.collider.gameObject);
                        return "Grabbed a decal.";
                    }
                }

                if (args[0] == "glass")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.Glass) || Physics.Raycast(ray, out hit, float.MaxValue, (int)LayerType.BreakableGlass))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out BreakableWindow w) 
                            || hit.transform.gameObject.TryGetComponent(out w) 
                            || hit.transform.parent.gameObject.TryGetComponent(out w) || hit.rigidbody.TryGetComponent(out w))
                        {
                            grab.Grab(w.GetWindow());
                            return $"Grabbed a window.";
                        }
                        else
                        {
                            Log.Error("Grabber", "Raycast has succesfully hit a Glass Layer, but the Window does not have a BreakableWindow component.");
                            grab.Grab(hit.collider.gameObject);
                            return "Raycast has succesfully hit a Glass Layer, but the Window does not have a BreakableWindow component.";
                        }
                    }
                }

                if (args[0] == "cctv")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.CCTV))
                    {
                        grab.Grab(hit.collider.gameObject);
                        return $"Grabbed a CCTV.";
                    }
                }

                if (args[0] == "grenade")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.Grenade) 
                        || Physics.Raycast(ray, out hit, float.MaxValue, (int)LayerType.Scp018))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out Grenades.Grenade nade) 
                            || hit.transform.gameObject.TryGetComponent(out nade) 
                            || hit.transform.parent.TryGetComponent(out nade) || hit.rigidbody.TryGetComponent(out nade))
                        {
                            grab.Grab(nade.gameObject);
                            return $"Grabbed a {nade.logName} grenade";
                        }
                        else
                        {
                            Log.Error("Grabber", "Raycast has succesfully hit a Grenade Layer, but the Grenade does not have a Grenade component.");
                            grab.Grab(hit.collider.gameObject);
                            return "Raycast has succesfully hit a Grenade Layer, but the Grenade does not have a Grenade component.";
                        }
                    }
                }

                if (args[0] == "ragdoll")
                {
                    Ray ray = new Ray
                    {
                        direction = sender.PlayerCamera.forward,
                        origin = sender.Position + sender.PlayerCamera.forward
                    };

                    if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, (int)LayerType.Ragdoll))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out Ragdoll r) || hit.transform.gameObject.TryGetComponent(out r) 
                            || hit.transform.parent.gameObject.TryGetComponent(out r) || hit.rigidbody.TryGetComponent(out r))
                        {
                            grab.Grab(r);
                            return $"Grabbed a ragdoll of {r.GetRole()}";
                        }
                        else
                        {
                            Log.Error("Grabber", "Raycast has succesfully hit a Ragdoll Layer, but the Ragdoll does not have a Ragdoll component.");
                            grab.Grab(hit.collider.gameObject);
                            return "Raycast has succesfully hit a Ragdoll Layer, but the Ragdoll does not have a Ragdoll component.";
                        }
                    }
                }
            }

            Log.Debug("Grabber", $"{sender.Nick} is trying to grab a {obj.name} ({obj.tag})");

            if (obj.CompareTag("Player"))
            {
                if (obj.TryGetComponent(out ReferenceHub hub))
                {
                    Player player = PlayersList.GetPlayer(hub);

                    if (player != null)
                    {
                        grab.Grab(player);
                        return $"Grabbed {player.Nick}";
                    }
                    else
                    {
                        Log.Error("Grabber", "GameObject has a valid ReferenceHub component, but the Player could not be found. (A-2)");
                        return "GameObject has a valid ReferenceHub component, but the Player could not be found. (A-2)";
                    }
                }
                else
                {
                    Log.Error("Grabber", "GameObject has a valid player tag, but does not have a ReferenceHub component (A-1).");
                    return "GameObject has a valid player tag, but does not have a ReferenceHub component (A-1).";
                }
            }

            if (obj.CompareTag("Pickup"))
            {
                if (obj.TryGetComponent(out Pickup item))
                {
                    grab.Grab(item);
                    return $"Grabbed {item.ItemId}";
                }
                else
                {
                    Log.Error("Grabber", "GameObject has a valid Pickup tag, but does not have a Pickup component (B-1).");
                    return "GameObject has a Pickup tag, but does not have a Pickup component (B-1).";
                }
            }

            BreakableWindow window = null;

            if (obj.CompareTag("Window") || obj.CompareTag("window") || obj.CompareTag("Glass") || obj.TryGetComponent(out window))
            {
                if (window == null && obj.TryGetComponent(out window))
                {
                    Window w = window.GetWindow();

                    if (w == null)
                    {
                        Log.Error("Grabber", "GameObject has a valid BreakableWindow component, but the Window could not be found. (C-2)");
                        return "GameObject has a valid BreakableWindow component, but the Window could not be found. (C-2)";
                    }
                    else
                    {
                        grab.Grab(w);
                        return "Grabbed a window.";
                    }
                }
                else
                {
                    if (window != null)
                    {
                        Window w = window.GetWindow();

                        if (w == null)
                        {
                            Log.Error("Grabber", "GameObject has a valid BreakableWindow component, but the Window could not be found. (C-2)");
                            return "GameObject has a valid BreakableWindow component, but the Window could not be found. (C-2)";
                        }
                        else
                        {
                            grab.Grab(w);
                            return "Grabbed a window.";
                        }
                    }
                    else
                    {
                        Log.Error("Grabber", "GameObject has a valid Window tag, but does not have a BreakableWindow component (C-1).");
                        return "GameObject has a Window tag, but does not have a BreakableWindow component (C-1).";
                    }
                }
            }

            if (obj.CompareTag("workbench") || obj.CompareTag("Workbench") || obj.CompareTag("workstation") || obj.CompareTag("Workstation"))
            {
                if (obj.TryGetComponent(out WorkStation s))
                {
                    Workstation station = s.GetWorkstation();

                    if (station == null)
                    {
                        Log.Error("Grabber", "GameObject has a valid WorkStation component, but the Workstation could not be found. (E-2)");
                        return "GameObject has a valid WorkStation component, but the Workstation could not be found. (E-2)";
                    }
                    else
                    {
                        grab.Grab(station);
                        return "Grabbed a WorkStation";
                    }
                }
                else
                {
                    Log.Error("Grabber", "GameObject has a valid Workbench tag, but does not have a WorkStation component (E-1).");
                    return "GameObject has a Workbench tag, but does not have a WorkStation component (E-1).";
                }
            }


            Ragdoll rag = null;

            if (obj.CompareTag("Ragdoll") || obj.TryGetComponent(out rag))
            {
                if (rag == null && obj.TryGetComponent(out rag))
                {
                    grab.Grab(rag);
                    return "Grabbed a ragdoll.";
                }
                else
                {
                    if (rag != null)
                    {
                        grab.Grab(rag);
                        return "Grabbed a ragdoll.";
                    }
                    else
                    {
                        Log.Error("Grabber", "GameObject has a valid Ragdoll tag, but does not have a Ragdoll component (F-1).");
                        return "GameObject has a Ragdoll tag, but does not have a Ragdoll component (F-1).";
                    }
                }
            }

            grab.Grab(obj);
            return "Grabbed an optional object. Keep in mind that this might not be working.";
        }
    }
}