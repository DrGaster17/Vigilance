using Vigilance.API;
using Vigilance.Extensions.Rpc;

using UnityEngine;
using Mirror;

namespace Vigilance.Plugins.Commands.Components
{
    public class Grabber : MonoBehaviour
    {
        private RoleType _grabbedRole;
        private float _grabbedRot;

        public ReferenceHub player;

        public Pickup pickup;
        public ReferenceHub grabbed;
        public Window window;
        public Workstation station;
        public GameObject opt;
        public Generator gen;
        public Ragdoll rag;
        public API.Door door;

        public bool modified;
        public bool allow;

        public bool isGrabbing => player != null && (pickup != null || grabbed != null || window != null || station != null || opt != null || rag != null || door != null) && allow;

        public Vector3 grabPos => player.playerMovementSync.RealModelPosition + player.PlayerCameraReference.forward * 2;
        public Quaternion grabRot => player.PlayerCameraReference.rotation;

        public void Start()
        {
            player = ReferenceHub.GetHub(gameObject);
        }

        public void Update()
        {
            if (isGrabbing)
            {
                if (pickup != null)
                {
                    pickup.transform.position = grabPos;
                    pickup.transform.rotation = grabRot;
                }

                if (grabbed != null)
                {
                    grabbed.playerMovementSync.OverridePosition(grabPos, _grabbedRot, false);
                }

                if (window != null)
                {
                    window.Position = grabPos;
                    window.Rotation = grabRot;
                }

                if (station != null)
                {
                    station.Position = grabPos;
                    station.Rotation = grabRot;
                }

                if (opt != null)
                {
                    NetworkServer.UnSpawn(opt);
                    opt.transform.position = grabPos;
                    opt.transform.rotation = grabRot;
                    NetworkServer.Spawn(opt);
                }

                if (rag != null)
                {
                    rag.transform.position = grabPos;
                    rag.transform.rotation = grabRot;
                }

                if (gen != null)
                {
                    gen.Position = grabPos;
                    gen.Rotation = grabRot;
                }

                if (door != null)
                {
                    door.Position = grabPos;
                    door.Rotation = grabRot;
                }
            }
        }

        public void OnDestroy()
        {
            Stop();
        }

        public void Stop()
        {
            allow = false;

            pickup = null;
            window = null;
            station = null;
            opt = null;
            gen = null;
            door = null;

            if (grabbed != null)
            {
                Player ply = PlayersList.GetPlayer(grabbed);

                if (ply != null)
                {
                    ply.ChangeAppearance(_grabbedRole);

                    ply.ReferenceHub.playerMovementSync.NoclipWhitelisted = false;
                    ply.ReferenceHub.playerMovementSync.WhitelistPlayer = false;
                }
            }

            _grabbedRot = 0f;
            _grabbedRole = RoleType.None;

            grabbed = null;
        }

        public void Grab(Pickup pickup)
        {
            allow = true;
            this.pickup = pickup;
        }

        public void Grab(Player grabbed)
        {
            _grabbedRole = grabbed.Role;
            _grabbedRot = grabbed.PlayerCamera.forward.x;

            allow = true;
            this.grabbed = grabbed.ReferenceHub;

            grabbed.ReferenceHub.playerMovementSync.WhitelistPlayer = true;
            grabbed.ReferenceHub.playerMovementSync.NoclipWhitelisted = true;
            grabbed.ChangeAppearance(RoleType.Tutorial);
        }

        public void Grab(Window window)
        {
            allow = true;
            this.window = window;
        }

        public void Grab(Workstation station)
        {
            allow = true;
            this.station = station;
        }

        public void Grab(GameObject opt)
        {
            allow = true;
            this.opt = opt;
        }

        public void Grab(Ragdoll rag)
        {
            allow = true;
            this.rag = rag;
        }

        public void Grab(Generator gen)
        {
            allow = true;
            this.gen = gen;
        }

        public void Grab(API.Door door)
        {
            allow = true;
            this.door = door;
        }
    }
}
