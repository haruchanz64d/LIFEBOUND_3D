using LB.Character;
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            lastPlatformPosition = transform.position;

            if (IsServer)
            {
                // Call RPCs only on server
                foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
                {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
                    foreach (var collider in colliders)
                    {
                        if (collider.CompareTag("Player") && collider.GetComponent<NetworkObject>() != null)
                        {
                            SetPlayerParentToMovingPlatformRpc(collider.GetComponent<NetworkObject>().NetworkObjectId);
                            break;
                        }
                    }
                }
            }
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

        /// <summary>
        /// OnTriggerEnter is called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="other">The other Collider involved in this collision.</param>
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetPlayerParentToMovingPlatformRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }

        /// <summary>
        /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
        /// </summary>
        /// <param name="other">The other Collider involved in this collision.</param>
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                RemovePlayerParentFromMovingPlatformRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void SetPlayerParentToMovingPlatformRpc(ulong playerNetworkObjectId)
        {
            if (IsServer)
            {
                NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
                player.TryGetComponent<Player>(out Player playerScript);
                if (playerScript != null)
                {
                    playerScript.transform.SetParent(transform, true); // Set parent with world position and rotation
                }
            }
        }


        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void RemovePlayerParentFromMovingPlatformRpc(ulong playerNetworkObjectId)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            player.TryGetComponent<Player>(out Player playerScript);
            if (playerScript != null)
            {
                playerScript.transform.SetParent(null); // Remove parent
            }
        }

    }
}
