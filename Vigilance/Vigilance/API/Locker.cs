using System.Collections.Generic;
using System.Linq;
using Vigilance.External.Utilities;
using UnityEngine;

namespace Vigilance.API
{
    public class Locker
    {
        private global::Locker _locker;

        public Locker(global::Locker locker, byte lockerId)
        {
            _locker = locker;
            LockerId = lockerId;
            Room = MapUtilities.FindParentRoom(locker.gameObject.gameObject);
            List<Chamber> chambers = new List<Chamber>();
            _locker.chambers.ToList().ForEach((x) => chambers.Add(new Chamber(x, (byte)_locker.chambers.IndexOf(x))));
            Chambers = chambers.AsReadOnly();
        }

        public Room Room { get; }
        public IReadOnlyCollection<Chamber> Chambers { get; }
        public byte LockerId { get; }

        public List<global::Locker.ItemToSpawn> ItemToSpawn => _locker._itemsToSpawn;
        public List<Pickup> Items => _locker._assignedPickups;

        public string Name { get => _locker.name; set => _locker.name = value; }
        public string Tag { get => _locker.lockerTag; set => _locker.lockerTag = value; }
        public bool Spawned { get => _locker.Spawned; set => _locker.Spawned = value; }
        public bool SpawnOnOpen { get => _locker.SpawnOnOpen; set => _locker.SpawnOnOpen = value; }
        public bool TriggeredByDoor { get => _locker.TriggeredByDoor; set => _locker.TriggeredByDoor = value; }
        public bool AnyVirtual { get => _locker.AnyVirtual; set => _locker.AnyVirtual = value; }
        public bool EnableSorting { get => _locker.enableSorting; set => _locker.enableSorting = value; }
        public int ChanceOfSpawn { get => _locker.chanceOfSpawn; set => _locker.chanceOfSpawn = value; }

        public GameObject GameObject => _locker.gameObject.gameObject;
        public Vector3 Position { get => _locker.gameObject.position; }
        public Vector3 Scale { get => _locker.gameObject.localScale; }
        public Quaternion Rotation { get => _locker.gameObject.rotation; }


        public Chamber GetChamber(byte chamberId)
        {
            foreach (Chamber chamber in Chambers)
            {
                if (chamber.ChamberId == chamberId)
                    return chamber;
            }

            if (!(chamberId >= Chambers.Count))
                return Chambers.ToList()[chamberId];

            return null;
        }
    }
}
