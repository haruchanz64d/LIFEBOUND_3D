using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class LBMovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float movementSpeed = 6.9f;
        private int currentWaypointIndex = 0;
        private Vector3 lastPlatformPosition;

        private void Start()
        {
            lastPlatformPosition = transform.position;
        }

        private void FixedUpdate()
        {
            MoveTowardsWaypointServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void MoveTowardsWaypointServerRpc()
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, movementSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

                if (currentWaypointIndex == 0 && isPlatformReachedEnd())
                {
                    SwitchBackToStartingPosition();
                }
            }
        }

        private void SwitchBackToStartingPosition()
        {
            transform.position = waypoints[0].position;
            currentWaypointIndex = 1;
        }

        private bool isPlatformReachedEnd()
        {
            return currentWaypointIndex == 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    SetPlayerParentToMovingPlatformServerRpc(networkObject.NetworkObjectId);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    RemovePlayerParentFromMovingPlatformServerRpc(networkObject.NetworkObjectId);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerParentToMovingPlatformServerRpc(ulong playerNetworkObjectId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            player.transform.SetParent(transform);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemovePlayerParentFromMovingPlatformServerRpc(ulong playerNetworkObjectId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            player.transform.SetParent(null);
        }
    }
}
