using UnityEngine;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Commands;

using Vigilance.Extensions;

using System.Collections.Generic;

using NorthwoodLib.Pools;

using Interactables.Interobjects.DoorUtils;

using Mirror;

namespace Vigilance.Plugins.Commands
{
    public class CommandObjectAction : ICommandHandler
    {
        public string Command => "objectaction";
        public string[] Aliases => new string[] { };

        public CommandType[] Environments => new CommandType[] { CommandType.RemoteAdmin };
        public CommandPermission Permission => CommandPermission.None;

        public void Delete(GameObject obj) => NetworkServer.Destroy(obj);

        public void MoveTo(Vector3 pos, GameObject obj)
        {
            NetworkServer.UnSpawn(obj);
            obj.transform.position = pos;
            NetworkServer.Spawn(obj);
        }

        public void Rotate(Quaternion rot, GameObject obj)
        {
            NetworkServer.UnSpawn(obj);
            obj.transform.rotation = rot;
            NetworkServer.Spawn(obj);
        }

        public void Resize(Vector3 scale, GameObject obj)
        {
            NetworkServer.UnSpawn(obj);
            obj.transform.localScale = scale;
            NetworkServer.Spawn(obj);
        }

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 1)
                return "Missing arguments!\nUsage: objectaction <action>\nUse objectaction list to obtain a list of all actions.";

            if (!CommandSelectObject.Objects.TryGetValue(sender, out GameObject obj) || obj == null)
                return "You need to select an object first.";

            if (args[0] == "list")
                return "info";

            if (args[0] == "info")
            {
                string s = "Object Info:\n";

                s += $"ID: {obj.GetInstanceID()}\n";
                s += $"Name: {obj.name}\n";
                s += $"Tag: {obj.tag}\n\n";

                s += $"Position: {obj.transform.position.AsString()}\n";
                s += $"Rotation: {obj.transform.rotation.eulerAngles.AsString()}\n";
                s += $"Scale: {obj.transform.localScale.AsString()}\n\n";

                s += $"Layer: {obj.layer}\n";
                s += $"LayerName: {(LayerType)obj.layer}\n\n";

                s += $"Components: {obj.GetComponents(typeof(Component)).Length}\n\n\n";

                s += $"Parent ID: {obj.transform.parent.GetInstanceID()}\n";
                s += $"Parent Name: {obj.transform.name}\n";
                s += $"Parent Tag: {obj.transform.parent.tag}\n\n";

                s += $"Parent Position: {obj.transform.parent.position.AsString()}\n";
                s += $"Parent Rotation: {obj.transform.parent.rotation.eulerAngles.AsString()}\n";
                s += $"Parent Scale: {obj.transform.parent.localScale.AsString()}\n\n";

                s += $"Parent Layer: {obj.transform.parent.gameObject.layer}\n";
                s += $"Parent LayerName: {(LayerType)obj.transform.parent.gameObject.layer}\n\n\n";

                s += $"Is Door: {obj.CompareTag("Door") || obj.TryGetComponent(out DoorVariant _)}";
                s += $"Is Pickup: {obj.CompareTag("Pickup") || obj.TryGetComponent(out Pickup _)}\n";
                s += $"Is Player: {obj.CompareTag("Player")}\n";
                s += $"Is Ragdoll: {obj.CompareTag("Ragdoll") || obj.TryGetComponent(out Ragdoll _)}\n";
                s += $"Is Room: {obj.CompareTag("Room") || obj.CompareTag("RoomID") || obj.TryGetComponent(out Rid _) || obj.TryGetComponent(out Room _) || obj.TryGetComponent(out RoomInformation _)}";

                return s;
            }

            if (args[0] == "components")
            {
                List<Component> comps = ListPool<Component>.Shared.Rent();

                obj.GetComponents(comps);

                string s = "Components:\n";

                foreach (Component comp in comps)
                {
                    s += $"--------- {comp.name} - {comp.tag} - START ---------\n";
                    s += $"ID: {comp.GetInstanceID()}\n";
                    s += $"Type: {comp.GetType().FullName}\n";

                    s += $"Position: {comp.transform.position.AsString()}\n";
                    s += $"Rotation: {comp.transform.rotation.eulerAngles.AsString()}\n";
                    s += $"Scale: {comp.transform.localScale.AsString()}\n\n";

                    s += $"Layer: {comp.gameObject.layer}\n";
                    s += $"LayerName: {(LayerType)comp.gameObject.layer}\n\n";

                    s += $"Is Door: {comp.CompareTag("Door") || comp.TryGetComponent(out DoorVariant _)}\n";
                    s += $"Is Pickup: {comp.CompareTag("Pickup") || comp.TryGetComponent(out Pickup _)}\n";
                    s += $"Is Player: {comp.CompareTag("Player")}\n";
                    s += $"Is Ragdoll: {comp.CompareTag("Ragdoll") || comp.TryGetComponent(out Ragdoll _)}\n";
                    s += $"Is Room: {comp.CompareTag("Room") || comp.CompareTag("RoomID") || comp.TryGetComponent(out Rid _) || comp.TryGetComponent(out Room _) || comp.TryGetComponent(out RoomInformation _)}\n";
                    s += $"--------- {comp.name} - {comp.tag} - END ---------\n";
                }

                ListPool<Component>.Shared.Return(comps);
                return s;
            }

            if (args[0] == "setpos")
            {
                if (args.Length < 2)
                {
                    MoveTo(sender.Position, obj);
                    return "Position set to your position.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Position must be a valid Vector3 in correct format - X,Y,Z.";

                MoveTo(vec.Value, obj);

                return $"Position set.";
            }

            if (args[0] == "setrot")
            {
                if (args.Length < 2)
                {
                    Rotate(sender.Rotations, obj);
                    return "Rotation set to your rotation.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Rotation must be a valid Vector3 in correct format - X,Y,Z.";

                Rotate(Quaternion.Euler(vec.Value), obj);

                return $"Rotation set.";
            }

            if (args[0] == "setscale")
            {
                if (args.Length < 2)
                {
                    Resize(sender.Scale, obj);
                    return "Scale set to your scale.";
                }

                if (!StringUtils.TryParseVector(args[1], out Vector3? vec) || !vec.HasValue)
                    return "Scale must be a valid Vector3 in correct format - X,Y,Z.";

                Resize(vec.Value, obj);

                return $"Scale set.";
            }

            if (args[0] == "delete")
            {
                Delete(obj);
                return "Object deleted.";
            }

            if (args[0] == "setActive")
            {
                if (args.Length < 2)
                    return "Missing arguments!\nUsage: \"objectaction setActive <active>\"";

                if (!bool.TryParse(args[1], out bool active))
                    return "\"active\" must be a valid boolean.";

                obj.SetActive(active);

                return "Done.";
            }

            return "Missing arguments!\nUsage: objectaction <action>\nUse \"objectaction list\" to obtain a list of all actions.";
        }
    }
}
