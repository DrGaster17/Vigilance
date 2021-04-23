using Mirror;
using UnityEngine;
using Vigilance.External.Utilities;
using Vigilance.External.Extensions;

namespace Vigilance.API
{
    public class Camera
    {
        private Camera079 _cam;

        public Camera(Camera079 cam)
        {
            _cam = cam;
            Room = MapUtilities.FindParentRoom(cam.gameObject);
            Type = cam.GetCameraType(); 
        }

        public Room Room { get; }
        public Enums.CameraType Type { get; }
        public ushort Id => _cam.cameraId;
        public string Name => _cam.cameraName;
        public GameObject GameObject => _cam.gameObject;
        public Vector3 Position => _cam.targetPosition.position;
        public Quaternion Rotations => _cam.targetPosition.rotation;
        public float Rotation => _cam.curRot;
        public bool IsMain => _cam.isMain;

        public void UpdatePosition(float rotation, float pitch) => _cam.UpdatePosition(rotation, pitch);

        public void SetPosition(Vector3 pos)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.position = pos;
            NetworkServer.Spawn(GameObject);
        }

        public void SetRotation(Quaternion rot)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.rotation = rot;
            NetworkServer.Spawn(GameObject);
        }

        public void SetScale(Vector3 scale)
        {
            NetworkServer.UnSpawn(GameObject);
            GameObject.transform.localScale = scale;
            NetworkServer.Spawn(GameObject);
        }
    }
}
