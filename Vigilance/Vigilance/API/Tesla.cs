using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Vigilance.Utilities;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public class Tesla
    {
        private TeslaGate _gate;
        public Tesla(TeslaGate gate)
        {
            _gate = gate;
            Room = MapUtilities.FindParentRoom(_gate.gameObject);
        }

        public bool IsDisabled { get; set; }
        public Room Room { get; }
        public GameObject GameObject => _gate.gameObject;

        public float SizeOfTrigger { get => _gate.sizeOfTrigger; set => _gate.sizeOfTrigger = value; }

        public Vector3 Position { get => _gate.transform.position; set => SetPosition(value); }
        public Vector3 Scale { get => _gate.transform.localScale; set => SetScale(value); }
        public Quaternion Rotation { get => _gate.transform.rotation; set => SetRotation(value); }

        public bool InProgress => _gate.InProgress;
        public bool ShowGizmos { get => _gate.showGizmos; set => _gate.showGizmos = value; }

        public void Burst()
        {
            _gate.ServerSideCode();
        }

        public void Electrocute(Player player)
        {
            _gate.ElectrocutePlayer(player.GameObject);
        }

        public void PlayAnimation()
        {
            _gate.RpcPlayAnimation();
        }

        public bool CanBeHurt(Player player)
        {
            return _gate.PlayerInHurtRange(player.GameObject);
        }

        public IEnumerable<Player> GetPlayersInRange(bool inHurtRange)
        {
            return _gate.PlayersInRange(inHurtRange).Select(x => x.GetPlayer());
        }

        private void SetPosition(Vector3 pos)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.position = pos;
            _gate.localPosition = pos;
            NetworkServer.Spawn(GameObject);
        }

        private void SetRotation(Quaternion rot)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.rotation = rot;
            _gate.localRotation = rot.eulerAngles;
            NetworkServer.Spawn(GameObject);
        }

        private void SetScale(Vector3 scale)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.localScale = scale;
            NetworkServer.Spawn(GameObject);
        }

        public static bool GatesDisabled { get; set; }
    }
}
