using System.Collections.Generic;
using System.Linq;
using Vigilance.API.Enums;
using UnityEngine;
using Interactables.Interobjects.DoorUtils;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public class Room : MonoBehaviour
    {
        public string Name => name;
        public Transform Transform => transform;
        public Vector3 Position => transform.position;

        public ZoneType Zone { get; private set; }
        public RoomType Type { get; private set; }

        public IEnumerable<Player> Players => PlayersList.List.Where(player => player.Room.Transform == Transform);
        public IEnumerable<Door> Doors { get; private set; }

        public bool LightsOff => FlickerableLightController && FlickerableLightController.IsEnabled();
        private FlickerableLightController FlickerableLightController { get; set; }

        public void TurnOffLights(float duration) => FlickerableLightController?.ServerFlickerLights(duration);
        public void SetLightIntensity(float intensity) => FlickerableLightController?.ServerSetLightIntensity(intensity);

        private static RoomType FindType(string rawName)
        {
            rawName = rawName.RemoveBracketsOnEndOfName();

            switch (rawName)
            {
                case "LCZ_Armory":
                    return RoomType.LczArmory;
                case "LCZ_Curve":
                    return RoomType.LczCurve;
                case "LCZ_Straight":
                    return RoomType.LczStraight;
                case "LCZ_012":
                    return RoomType.Lcz012;
                case "LCZ_914":
                    return RoomType.Lcz914;
                case "LCZ_Crossing":
                    return RoomType.LczCrossing;
                case "LCZ_TCross":
                    return RoomType.LczTCross;
                case "LCZ_Cafe":
                    return RoomType.LczCafe;
                case "LCZ_Plants":
                    return RoomType.LczPlants;
                case "LCZ_Toilets":
                    return RoomType.LczToilets;
                case "LCZ_Airlock":
                    return RoomType.LczAirlock;
                case "LCZ_173":
                    return RoomType.Lcz173;
                case "LCZ_ClassDSpawn":
                    return RoomType.LczClassDSpawn;
                case "LCZ_ChkpB":
                    return RoomType.LczCheckpointB;
                case "LCZ_372":
                    return RoomType.LczGlassBox;
                case "LCZ_ChkpA":
                    return RoomType.LczCheckpointA;
                case "HCZ_079":
                    return RoomType.Hcz079;
                case "HCZ_EZ_Checkpoint":
                    return RoomType.HczEzCheckpoint;
                case "HCZ_Room3ar":
                    return RoomType.HczArmory;
                case "HCZ_Testroom":
                    return RoomType.Hcz939;
                case "HCZ_Hid":
                    return RoomType.HczHid;
                case "HCZ_049":
                    return RoomType.Hcz049;
                case "HCZ_ChkpA":
                    return RoomType.HczCheckpointA;
                case "HCZ_Crossing":
                    return RoomType.HczCrossing;
                case "HCZ_106":
                    return RoomType.Hcz106;
                case "HCZ_Nuke":
                    return RoomType.HczNuke;
                case "HCZ_Tesla":
                    return RoomType.HczTesla;
                case "HCZ_Servers":
                    return RoomType.HczServers;
                case "HCZ_ChkpB":
                    return RoomType.HczCheckpointB;
                case "HCZ_Room3":
                    return RoomType.HczTCross;
                case "HCZ_457":
                    return RoomType.Hcz096;
                case "HCZ_Curve":
                    return RoomType.HczCurve;
                case "EZ_Endoof":
                    return RoomType.EzVent;
                case "EZ_Intercom":
                    return RoomType.EzIntercom;
                case "EZ_GateA":
                    return RoomType.EzGateA;
                case "EZ_PCs_small":
                    return RoomType.EzDownstairsPcs;
                case "EZ_Curve":
                    return RoomType.EzCurve;
                case "EZ_PCs":
                    return RoomType.EzPcs;
                case "EZ_Crossing":
                    return RoomType.EzCrossing;
                case "EZ_CollapsedTunnel":
                    return RoomType.EzCollapsedTunnel;
                case "EZ_Smallrooms2":
                    return RoomType.EzConference;
                case "EZ_Straight":
                    return RoomType.EzStraight;
                case "EZ_Cafeteria":
                    return RoomType.EzCafeteria;
                case "EZ_upstairs":
                    return RoomType.EzUpstairsPcs;
                case "EZ_GateB":
                    return RoomType.EzGateB;
                case "EZ_Shelter":
                    return RoomType.EzShelter;
                case "PocketWorld":
                    return RoomType.PocketDimension;
                case "Outside":
                    return RoomType.Surface;
                default:
                    return RoomType.Unknown;
            }
        }

        private static ZoneType FindZone(GameObject gameObject)
        {
            var transform = gameObject.transform;

            if (transform.parent == null)
                return ZoneType.Unspecified;

            switch (transform.parent.name)
            {
                case "HeavyRooms":
                    return ZoneType.HeavyContainment;
                case "LightRooms":
                    return ZoneType.LightContainment;
                case "EntranceRooms":
                    return ZoneType.EntranceZone;
                default:
                    return transform.position.y > 900 ? ZoneType.SurfaceZone : ZoneType.Unspecified;
            }
        }

        private static List<Door> FindDoors(GameObject gameObject)
        {
            List<Door> apiDoors = new List<Door>();

            foreach (Scp079Interactable scp079Interactable in Interface079.singleton.allInteractables)
            {
                foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interactable.currentZonesAndRooms)
                {
                    if (zoneAndRoom.currentRoom == gameObject.name && zoneAndRoom.currentZone == gameObject.transform.parent.name)
                    {
                        if (scp079Interactable.type == Scp079Interactable.InteractableType.Door)
                        {
                            DoorVariant variant = scp079Interactable.GetComponent<DoorVariant>();

                            Door door = variant.GetDoor();

                            if (door != null)
                            {
                                if (!apiDoors.Contains(door))
                                {
                                    apiDoors.Add(door);
                                }
                            }
                        }
                    }
                }
            }

            return apiDoors;
        }

        private void Start()
        {
            Zone = FindZone(gameObject);
            Type = FindType(gameObject.name);
            Doors = FindDoors(gameObject);
            FlickerableLightController = GetComponentInChildren<FlickerableLightController>();
        }
    }
}
