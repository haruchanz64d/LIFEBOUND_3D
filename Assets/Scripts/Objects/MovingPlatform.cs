using LB.Character;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class MovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        private float movementSpeed = 8f;
        private int currentWaypointIndex = 0;

        [SerializeField] private NetworkObject player;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
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
            if (IsServer)
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.transform.SetParent(transform);
                }
            }
            else
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    RequestParentToPlatformServerRpc(networkObject.NetworkObjectId);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsServer)
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.transform.SetParent(null);
                }
            }
            else
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    RequestUnparentFromPlatformServerRpc(networkObject.NetworkObjectId);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestParentToPlatformServerRpc(ulong playerNetworkObjectId)
        {
            NetworkObject playerNetworkObject = GetNetworkObject(playerNetworkObjectId);
            if (playerNetworkObject != null)
            {
                playerNetworkObject.transform.SetParent(transform);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestUnparentFromPlatformServerRpc(ulong playerNetworkObjectId)
        {
            NetworkObject playerNetworkObject = GetNetworkObject(playerNetworkObjectId);
            if (playerNetworkObject != null)
            {
                playerNetworkObject.transform.SetParent(null);
            }
        }
    }
}
