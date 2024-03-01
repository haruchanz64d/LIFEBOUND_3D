using UnityEngine;
using Unity.Netcode;
using Cinemachine;

namespace LB.CCamera
{
    public class LBCamera : NetworkBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLook;
        [SerializeField] private AudioListener listener;
        private void Awake()
        {
            freeLook = GetComponent<CinemachineFreeLook>();
            listener = GetComponent<AudioListener>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                listener.enabled = true;
                freeLook.Priority = 1;
            }
            else
            {
                freeLook.Priority = 0;
            }
        }
    }
}