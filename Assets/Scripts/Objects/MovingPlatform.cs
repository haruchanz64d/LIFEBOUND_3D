// MovingPlatform.cs
using LB.Character;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class MovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        private float movementSpeed = 20f;
        private int currentWaypointIndex = 0;

        private List<NetworkObject> players = new List<NetworkObject>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                NetworkObject player = other.GetComponent<NetworkObject>();
                player.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                NetworkObject player = other.GetComponent<NetworkObject>();
                player.RemoveOwnership();
            }
        }

        private void Update()
        {
            if (Vector3.Distance(waypoints[currentWaypointIndex].position, transform.position) < 0.1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, movementSpeed * Time.deltaTime);

            foreach (var player in players)
            {
                UpdatePlayerPositionClientRpc(player.NetworkObjectId, player.transform.position);
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