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

        public NetworkVariable<bool> hasPlayer = new NetworkVariable<bool>(false);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

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

        /// <summary>
        /// OnTriggerEnter is called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="other">The other Collider involved in this collision.</param>
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    SetPlayerParentToMovingPlatformServerRpc(player.NetworkObjectId);
                    player.SetMovingPlatform(this);
                }
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
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.SetMovingPlatform(null);
                    SetPlayerParentToMovingPlatformServerRpc(default);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerParentToMovingPlatformServerRpc(ulong playerid)
        {
            if(playerid == default)
            {
                hasPlayer.Value = false;
            }
            else 
            {
                hasPlayer.Value = true;
            }
        }
    }
}
