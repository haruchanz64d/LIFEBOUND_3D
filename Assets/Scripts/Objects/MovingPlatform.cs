using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class MovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        private float movementSpeed = 10f;
        private int currentWaypointIndex = 0;


        private void Update()
        {
            if (IsServer)
            {
               RequestParentToPlatformServerRpc();
                RequestUnparentFromPlatformServerRpc();
            }
            if (IsClient)
            {
                ParentPlayerToPlatformClientRpc();
                UnparentPlayerFromPlatformClientRpc();
            }
        }
        private void FixedUpdate()
        {
            MovePlatform();
        }

        private void MovePlatform()
        {
            if (Vector3.Distance(waypoints[currentWaypointIndex].position, transform.position) < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }

            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, movementSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                if(IsServer) { AttachToPlatform(networkObject); }
                if(IsClient) { RequestParentToPlatformServerRpc(); }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                if(IsServer) { DetachFromPlatform(networkObject); }
                if(IsClient) { RequestUnparentFromPlatformServerRpc(); }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestParentToPlatformServerRpc()
        {
            ParentPlayerToPlatformClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestUnparentFromPlatformServerRpc()
        {
            UnparentPlayerFromPlatformClientRpc();
        }

        [ClientRpc]
        private void ParentPlayerToPlatformClientRpc()
        {
            AttachToPlatform(GetComponent<NetworkObject>());
        }

        [ClientRpc]
        private void UnparentPlayerFromPlatformClientRpc()
        {
            DetachFromPlatform(GetComponent<NetworkObject>());
        }

        private void AttachToPlatform(NetworkObject networkObject)
        {
            networkObject.transform.SetParent(transform);
        }

        private void DetachFromPlatform(NetworkObject networkObject)
        {
            networkObject.transform.SetParent(null);
        }
    }
}
