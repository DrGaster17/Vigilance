using Mirror;
using UnityEngine;
using Vigilance.Utilities;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public class Workstation
    {
        private WorkStation _station;

        public Workstation(WorkStation stat)
        {
            _station = stat;
            IsLocked = false;
            Room = MapUtilities.FindParentRoom(_station.gameObject);
        }

        public Room Room { get; }
        public bool IsLocked { get; set; }
        public float MaxDistance { get => _station.maxDistance; set => _station.maxDistance = value; }
        public bool IsConnected { get => _station.NetworkisTabletConnected; set => _station.NetworkisTabletConnected = value; }
        public Player TabletOwner { get => IsConnected ? _station.Network_playerConnected.GetPlayer() : null; set => ConnectTablet(value); }

        public Vector3 Position { get => _station.transform.position; set => SetPosition(value); }
        public Vector3 Scale { get => _station.transform.localScale; set => SetScale(value); }
        public Quaternion Rotation { get => _station.transform.rotation; set => SetRotation(value); }

        public GameObject GameObject => _station.gameObject;


        public bool CanUse(Player player) => !IsLocked && CanPlace(player);
        public bool CanPlace(Player player) => _station.CanPlace(player.GameObject);
        public bool CanTake(Player player) => _station.CanTake(player.GameObject);
        public bool HasTablet(Player player) => _station.HasInInventory(player.GameObject);
        public void ConnectTablet(Player player = null) => _station.ConnectTablet(player == null ? CompCache.Player.GameObject : player.GameObject);

        private void SetPosition(Vector3 pos)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.position = pos;
            NetworkServer.Spawn(GameObject);
        }

        private void SetRotation(Quaternion rot)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.rotation = rot;
            NetworkServer.Spawn(GameObject);
        }

        private void SetScale(Vector3 scale)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.localScale = scale;
            NetworkServer.Spawn(GameObject);
        }
    }
}
