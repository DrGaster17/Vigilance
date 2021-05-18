using UnityEngine;
using Mirror;
using Vigilance.Utilities;

namespace Vigilance.API
{
    public class Window
    {
        private BreakableWindow _w;

        public Window(BreakableWindow w)
        {
            _w = w;
            Room = MapUtilities.FindParentRoom(w.gameObject);
            InstanceId = w.GetInstanceID();
        }

        public Room Room { get; }
        public int InstanceId { get; }

        public float Health { get => _w.health; set => _w.health = value; }
        public bool IsBroken { get => _w.NetworksyncStatus.broken; set => DestroyByBoolean(value); }

        public Vector3 Position { get => _w.syncStatus.position; set => SetPosition(value); }
        public Quaternion Rotation { get => _w.syncStatus.rotation; set => SetRotation(value); }

        public GameObject GameObject => _w.gameObject;

        public void EnableColliders() => _w.EnableColliders();
        public void DisableColliders() => SetColliders(false);

        public void Break() => IsBroken = true;

        public void SetPosition(Vector3 pos)
        {
            BreakableWindow.BreakableWindowStatus stat = _w.NetworksyncStatus;
            stat.position = pos;
            _w.UpdateStatus(stat);
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.position = pos;
            NetworkServer.Spawn(GameObject);
        }

        public void SetRotation(Quaternion rot)
        {
            BreakableWindow.BreakableWindowStatus stat = _w.NetworksyncStatus;
            stat.rotation = rot;
            _w.UpdateStatus(stat);
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.rotation = rot;
            NetworkServer.Spawn(GameObject);
        }

        public void SetColliders(bool value)
        {
            _w.GetComponent<Collider>().enabled = value;

            Collider[] colliders = _w.GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
            {
                collider.enabled = value;
            }
        }

        private void DestroyByBoolean(bool value)
        {
            if (!value)
                return;

            _w.ServerDamageWindow(Health + 10f);
        }
    }
}
