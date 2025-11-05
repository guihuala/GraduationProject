using UnityEngine;

namespace KidGame.Core
{
    public class CameraController : Singleton<CameraController>
    {
        public Transform Player;
        public float smoothSpeed = 20f;
        private Vector3 offsetVec3 = Vector3.zero;
        private float initialDistance;

        public void Init()
        {
            if (Player == null)
                Player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (Player == null) return;
            initialDistance = Vector3.Distance(transform.position, Player.position);
        }

        private void LateUpdate()
        {
            if (Player == null) return;

            // Vector3 desiredPosition = Player.position - transform.forward * initialDistance;
            //
            // transform.position = Vector3.Lerp(transform.position, desiredPosition + offsetVec3,
            //     Time.deltaTime * smoothSpeed);
        }
    }
}