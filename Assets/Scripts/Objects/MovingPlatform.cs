using LB.Character;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class MovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        private float movementSpeed = 10f;
        private int currentWaypointIndex = 0;

        private Vector3 lastPosition;

        private void Start()
        {
            lastPosition = transform.position;
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
                if (IsServer)
                {
                    AttachToPlatform(networkObject);
                }
                else
                {
                    RequestParentToPlatformServerRpc(networkObject.NetworkObjectId);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                if (IsServer)
                {
                    DetachFromPlatform(networkObject);
                }
                else
                {
                    RequestUnparentFromPlatformServerRpc(networkObject.NetworkObjectId);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestParentToPlatformServerRpc(ulong playerNetworkObjectId)
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            if (playerNetworkObject != null)
            {
                AttachToPlatform(playerNetworkObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestUnparentFromPlatformServerRpc(ulong playerNetworkObjectId)
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            if (playerNetworkObject != null)
            {
                DetachFromPlatform(playerNetworkObject);
            }
        }

        private void AttachToPlatform(NetworkObject networkObject)
        {
            networkObject.transform.SetParent(transform);

            CharacterController controller = networkObject.GetComponent<CharacterController>();
            if (controller != null)
            {
                StartCoroutine(MovePlayerWithPlatform(controller));
            }
        }

        private void DetachFromPlatform(NetworkObject networkObject)
        {
            networkObject.transform.SetParent(null);

            CharacterController controller = networkObject.GetComponent<CharacterController>();
            if (controller != null)
            {
                StopCoroutine(MovePlayerWithPlatform(controller));
            }
        }

        private IEnumerator MovePlayerWithPlatform(CharacterController controller)
        {
            while (true)
            {
                Vector3 movement = transform.position - lastPosition;
                lastPosition = transform.position;

                controller.Move(movement);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
