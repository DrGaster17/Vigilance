using System;
using System.Collections.Generic;

using Vigilance.API;

using Vigilance.Extensions;

using UnityEngine;

namespace Vigilance.Utilities
{
    public static class MapCache
    {
        private static string _outsitePanelName = "OutsitePanelScript";
        private static string _femurName = "FemurBreaker";
        private static string _intercomName = "IntercomSpeakingZone";
        private static string _pocketName = "HeavyRooms/PocketWorld";
        private static string _roomTag = "Room";
        private static string _pdExitTag = "PD_EXIT";
        private static string _surfaceTag = "Outside";

        private static AlphaWarheadController _awc;
        private static AlphaWarheadNukesitePanel _nukesite;
        private static AlphaWarheadOutsitePanel _outsite;
        private static OneOhSixContainer _ooc;
        private static LureSubjectContainer _cont;
        private static GameObject _outsitePanelScript;
        private static GameObject _femurBreaker;
        private static GameObject _intercomZone;
        private static GameObject _pocket;
        private static GameObject _surface;

        private static List<GameObject> _roomsObjects = new List<GameObject>();
        private static List<FlickerableLight> _lights = new List<FlickerableLight>();
        private static List<FlickerableLightController> _controllers = new List<FlickerableLightController>();
        private static List<GameObject> _pocketExits = new List<GameObject>();
        private static List<Room> _rooms = new List<Room>();

        public static GameObject OutsitePanelScript => _outsitePanelScript == null ? (_outsitePanelScript = GameObject.Find(_outsitePanelName)) : _outsitePanelScript;
        public static GameObject FemurBreaker => _femurBreaker == null ? (_femurBreaker = GameObject.Find(_femurName)) : _femurBreaker;
        public static GameObject IntercomZone => _intercomZone == null ? (_intercomZone = GameObject.Find(_intercomName)) : _intercomZone;
        public static GameObject PocketDimension => _pocket == null ? (_pocket = GameObject.Find(_pocketName)) : _pocket;
        public static GameObject Surface => _surface == null ? (_surface = GameObject.Find(_surfaceTag)) : _surface;
        public static AlphaWarheadController AlphaWarhead => _awc == null ? (_awc = AlphaWarheadController.Host) : _awc;
        public static AlphaWarheadNukesitePanel Nukesite => _nukesite == null ? (_nukesite = AlphaWarheadOutsitePanel.nukeside) : _nukesite;
        public static AlphaWarheadOutsitePanel Outsite => _outsite == null ? (_outsite = OutsitePanelScript.GetComponent<AlphaWarheadOutsitePanel>()) : _outsite;
        public static OneOhSixContainer OneOhSixContainer => _ooc == null ? (_ooc = MapUtilities.Find<OneOhSixContainer>()) : _ooc;
        public static LureSubjectContainer LureSubjectContainer => _cont == null ? (_cont = MapUtilities.Find<LureSubjectContainer>()) : _cont;

        public static IEnumerable<GameObject> RoomObjects => _roomsObjects;
        public static IEnumerable<FlickerableLight> Lights => _lights;
        public static IEnumerable<FlickerableLightController> Controllers => _controllers;
        public static IEnumerable<GameObject> PdExits => _pocketExits;
        public static IEnumerable<Room> Rooms => _rooms;

        public static Ragdoll.Info DefaultRagdollOwner { get; } = new Ragdoll.Info()
        {
            ownerHLAPI_id = null,
            PlayerId = -1,
            DeathCause = new PlayerStats.HitInfo(-1f, "[REDACTED]", DamageTypes.Com15, -1),
            ClassColor = new Color(1f, 0.556f, 0f),
            FullName = "Class-D",
            Nick = "[REDACTED]",
        };

        public static void Refresh()
        {
            try
            {
                _outsitePanelScript = GameObject.Find(_outsitePanelName);
                _femurBreaker = GameObject.FindGameObjectWithTag(_femurName);
                _intercomZone = GameObject.Find(_intercomName);

                _pocket = GameObject.Find(_pocketName);

                _outsite = _outsitePanelScript.GetComponent<AlphaWarheadOutsitePanel>();

                _awc = AlphaWarheadController.Host;
                _nukesite = AlphaWarheadOutsitePanel.nukeside;

                _ooc = MapUtilities.Find<OneOhSixContainer>();
                _cont = MapUtilities.Find<LureSubjectContainer>();

                _roomsObjects.AddRange(GameObject.FindGameObjectsWithTag(_roomTag));
                _roomsObjects.Add(PocketDimension);
                _roomsObjects.Add(Surface);

                _lights.AddRange(MapUtilities.FindObjects<FlickerableLight>());
                _controllers.AddRange(MapUtilities.FindObjects<FlickerableLightController>());
                _pocketExits.AddRange(GameObject.FindGameObjectsWithTag(_pdExitTag));

                _roomsObjects.ForEach((x) => _rooms.Add(x.AddComponent<Room>()));

                DoorExtensions.SetInfo();
                WorkstationExtensions.SetInfo();
                CameraExtensions.SetInfo();
                LiftExtensions.SetInfo();
                TeslaExtensions.SetInfo();
                GeneratorExtensions.SetInfo();
                LockerExtensions.SetInfo();
                WindowExtensions.SetInfo();
            }
            catch (Exception e)
            {
                Log.Add("MapCache", $"An error occured while refreshing cache!\n{e}", LogType.Error);
            }
        }

        public static void RefreshPdExits()
        {
            _pocketExits.Clear();
            _pocketExits.AddRange(GameObject.FindGameObjectsWithTag(_pdExitTag));
        }

        public static void Clear()
        {
            try
            {
                _roomsObjects.Clear();
                _lights.Clear();
                _controllers.Clear();
                _pocketExits.Clear();
                _rooms.Clear();

                DoorExtensions.Doors.Clear();
                DoorExtensions.Types.Clear();
                WorkstationExtensions.Workstations.Clear();
                CameraExtensions.Types.Clear();
                CameraExtensions.Cameras.Clear();
                LiftExtensions.Elevators.Clear();
                LiftExtensions.OrderedElevatorTypes.Clear();
                TeslaExtensions.Gates.Clear();
                GeneratorExtensions.Generators.Clear();
                LockerExtensions.Lockers.Clear();
                WindowExtensions.Windows.Clear();

                MapUtilities.ClearDummies();
            }
            catch (Exception e)
            {
                Log.Add("MapCache", $"An error occured while clearing the cache!\n{e}", LogType.Error);
            }
        }
    }
}
