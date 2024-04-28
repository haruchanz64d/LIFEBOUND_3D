using LB.Character;
using Unity.Netcode;
using UnityEngine;

namespace LB.Environment.Objects
{
    public class LBMovingPlatform : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float movementSpeed = 9.0f;
        private int currentWaypointIndex = 0;

        public NetworkVariable<bool> hasPlayer = new NetworkVariable<bool>(false);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }
        private void Update()
        {
            if(Vector3.Distance(waypoints[currentWaypointIndex].position, transform.position) < 0.1f)
            {
                currentWaypointIndex++;
                if(currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, movementSpeed * Time.deltaTime);
        }

        /// <summary>
        /// OnTriggerEnter is called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="other">The other Collider involved in this collision.</param>
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetPlayerParentToMovingPlatformServerRpc(other.GetComponent<Player>().NetworkObjectId);
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
                RemovePlayerParentFromMovingPlatformServerRpc(other.GetComponent<Player>().NetworkObjectId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerParentToMovingPlatformServerRpc(ulong playerid)
        {
            var player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerid].GetComponent<Player>();
            player.transform.SetParent(transform);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemovePlayerParentFromMovingPlatformServerRpc(ulong playerid)
        {
            var player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerid].GetComponent<Player>();
            player.transform.SetParent(null);
        }
    }
}
