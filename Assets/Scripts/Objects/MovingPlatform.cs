using LB.Character;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class MovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        private float movementSpeed = 10f;
        private int currentWaypointIndex = 0;

        private List<Transform> playersOnPlatform = new List<Transform>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        private void Update()
        {
            MovePlatform();

            foreach (var playerTransform in playersOnPlatform)
            {
                // Calculate the position of the player relative to the platform
                Vector3 newPosition = playerTransform.position + (transform.position - transform.parent.position);
                // Sync player position to all clients
                UpdatePlayerPositionClientRpc(playerTransform.GetComponent<NetworkObject>().NetworkObjectId, newPosition);
            }
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
            if (other.CompareTag("Player"))
            {
                Transform playerTransform = other.transform;
                playersOnPlatform.Add(playerTransform);
                // Set platform as parent to sync movement
                playerTransform.SetParent(transform);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Transform playerTransform = other.transform;
                playersOnPlatform.Remove(playerTransform);
                // Unset platform as parent
                playerTransform.SetParent(null);
            }
        }

        [ClientRpc]
        private void UpdatePlayerPositionClientRpc(ulong playerNetId, Vector3 position)
        {
            var playerNetObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetId];
            playerNetObj.transform.position = position;
        }
    }
}
