using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.Custom.Items.API;
using Vigilance.Custom.Items.Enums;
using UnityEngine;
using Interactables.Interobjects.DoorUtils;
using Vigilance.Serializable;

namespace Vigilance.Custom.Items.Extensions
{
    public static class Extensions
    {
        public static readonly SpawnPosition[] ReversedLocations =
        {
            SpawnPosition.InsideServersBottom,
            SpawnPosition.InsideHczArmory,
            SpawnPosition.Inside079First,
            SpawnPosition.InsideHidRight,
            SpawnPosition.Inside173Gate,
            SpawnPosition.InsideHidLeft,
            SpawnPosition.InsideGateA,
            SpawnPosition.InsideGateB,
            SpawnPosition.InsideLczWc,
            SpawnPosition.InsideGr18,
            SpawnPosition.Inside914,
            SpawnPosition.InsideHid,
        };

        public static Transform GetDoor(this SpawnPosition location)
        {
            string doorName = location.GetDoorName();

            if (string.IsNullOrEmpty(doorName))
                return null;

            return DoorNametagExtension.NamedDoors.TryGetValue(doorName, out var nametag) ? nametag.transform : null;
        }

        public static Vector3 GetPosition(this SpawnPosition location)
        {
            Transform transform = location.GetDoor();

            if (transform == null)
                return default;

            return transform.position + (Vector3.up * 1.5f) + (transform.forward * (ReversedLocations.Contains(location) ? -3f : 3f));
        }

        public static Vector ToVector(this Vector3 vector3) => new Vector(vector3.x, vector3.y, vector3.z);
        public static void ReloadWeapon(this Player player) => player.ReferenceHub.weaponManager.RpcReload(player.ReferenceHub.weaponManager.curWeapon);
        public static void Register(this IEnumerable<CustomItem> customItems)
        {
            if (customItems == null)
                throw new ArgumentNullException("customItems");

            foreach (CustomItem customItem in customItems)
                customItem.TryRegister();
        }

        public static void Unregister(this IEnumerable<CustomItem> customItems)
        {
            if (customItems == null)
                throw new ArgumentNullException("customItems");

            foreach (CustomItem customItem in customItems)
                customItem.TryUnregister();
        }

        public static string GetDoorName(this SpawnPosition spawnLocation)
        {
            switch (spawnLocation)
            {
                case SpawnPosition.Inside012:
                    return "012";
                case SpawnPosition.Inside096:
                    return "096";
                case SpawnPosition.Inside914:
                    return "914";
                case SpawnPosition.InsideHid:
                    return "HID";
                case SpawnPosition.InsideGr18:
                    return "GR18";
                case SpawnPosition.InsideGateA:
                    return "GATE_A";
                case SpawnPosition.InsideGateB:
                    return "GATE_B";
                case SpawnPosition.InsideLczWc:
                    return "LCZ_WC";
                case SpawnPosition.InsideHidLeft:
                    return "HID_LEFT";
                case SpawnPosition.InsideLczCafe:
                    return "LCZ_CAFE";
                case SpawnPosition.Inside173Gate:
                    return "173_GATE";
                case SpawnPosition.InsideIntercom:
                    return "INTERCOM";
                case SpawnPosition.InsideHidRight:
                    return "HID_RIGHT";
                case SpawnPosition.Inside079First:
                    return "079_FIRST";
                case SpawnPosition.Inside012Bottom:
                    return "012_BOTTOM";
                case SpawnPosition.Inside012Locker:
                    return "012_LOCKER";
                case SpawnPosition.Inside049Armory:
                    return "049_ARMORY";
                case SpawnPosition.Inside173Armory:
                    return "173_ARMORY";
                case SpawnPosition.Inside173Bottom:
                    return "173_BOTTOM";
                case SpawnPosition.InsideLczArmory:
                    return "LCZ_ARMORY";
                case SpawnPosition.InsideHczArmory:
                    return "HCZ_ARMORY";
                case SpawnPosition.InsideNukeArmory:
                    return "NUKE_ARMORY";
                case SpawnPosition.InsideSurfaceNuke:
                    return "SURFACE_NUKE";
                case SpawnPosition.Inside079Secondary:
                    return "079_SECONDARY";
                case SpawnPosition.Inside173Connector:
                    return "173_CONNECTOR";
                case SpawnPosition.InsideServersBottom:
                    return "SERVERS_BOTTOM";
                case SpawnPosition.InsideEscapePrimary:
                    return "ESCAPE_PRIMARY";
                case SpawnPosition.InsideEscapeSecondary:
                    return "ESCAPE_SECONDARY";
                default:
                    return default;
            }
        }
    }
}
