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
            MoveTowardsWaypoint();
        }

        private void MoveTowardsWaypoint()
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
                    SetPlayerParentToMovingPlatformRpc(networkObject.NetworkObjectId);
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
                    RemovePlayerParentFromMovingPlatformRpc(networkObject.NetworkObjectId);
                }
            }
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void SetPlayerParentToMovingPlatformRpc(ulong playerNetworkObjectId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            player.TrySetParent(transform);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void RemovePlayerParentFromMovingPlatformRpc(ulong playerNetworkObjectId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            player.transform.parent = null;
        }
    }
}
