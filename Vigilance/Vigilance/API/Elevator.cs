using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Vigilance.API.Enums;
using Vigilance.External.Extensions;
using Vigilance.External.Utilities;

namespace Vigilance.API
{
    public class Elevator
    {
        private Lift _lift;

        public Elevator(Lift lift)
        {
            _lift = lift;
            Room = MapUtilities.FindParentRoom(lift.gameObject);
            Type = lift.Type();
            DisallowedPlayers = new List<Player>();
        }

        public ElevatorType Type { get; }
        public Room Room { get; }
        public List<Player> DisallowedPlayers { get; }

        public GameObject GameObject => _lift.gameObject;
        public Vector3 Position => _lift.transform.position;
        public Quaternion Rotation => _lift.transform.rotation;
        public string Name => _lift.elevatorName;
        public IEnumerable<Lift.Elevator> Elevators => _lift.elevators;
        public bool IsLocked { get => _lift.Network_locked; set => _lift.Network_locked = value; }
        public Lift.Status Status { get => (Lift.Status)_lift.NetworkstatusID; set => _lift.NetworkstatusID = (byte)value; }
        public float MovingSpeed { get => _lift.movingSpeed; set => _lift.movingSpeed = value; }
        public float MaxDistance { get => _lift.maxDistance; set => _lift.maxDistance = value; }
        public bool Lockable { get => _lift.lockable; set => _lift.lockable = value; }
        public bool Operative { get => _lift.operative; set => _lift.operative = value; }

        public void CheckMeltPlayer(Player player) => _lift.CheckMeltPlayer(player.GameObject);
        public void InRange(Vector3 pos, out GameObject obj, float distanceMultiplier = 1f) => _lift.InRange(pos, out obj, distanceMultiplier, distanceMultiplier, distanceMultiplier);

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }

        public bool CanUse(Player player) => (!IsLocked && !player.PlayerLock && !DisallowedPlayers.Contains(player)) || player.BypassMode;

        public void SetLock(bool lockStatus)
        {
            _lift.SetLock(lockStatus);
        }

        public void SetStatus(byte status)
        {
            _lift.SetStatus(status);
        }

        public void PlayMusic()
        {
            _lift.RpcPlayMusic();
        }

        public void Use()
        {
            if (!IsLocked && Status != Lift.Status.Moving)
                _lift.UseLift();
        }
    }
}
