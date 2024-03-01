/// <summary>
/// This script implements a moving platform that activates and moves based on the number of players standing on it.
///
/// Key features:
/// - Moves along a set of waypoints defined in the Inspector.
/// - Has a maximum capacity for players.
/// - Activates only when the required number of players are standing on it:
///     - 1 player for maxCapacity = 1 or any value for maxCapacity > 1.
///     - 2 players for maxCapacity = 2.
/// - Deactivates and moves back to its starting position when:
///     - All players leave the platform while it's moving.
///     - It reaches its final waypoint (determined by `isPlatformReachedEnd`).
/// </summary>

using UnityEngine;

namespace LB.Environment.Objects
{
    public class LBMovingPlatform : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private int maxCapacity;
        [SerializeField] private GameObject platformObject;
        [SerializeField] private float movementSpeed = 6.9f;
        private int currentWaypointIndex = 0;
        private int currentPlayersOnPlatform = 0;
        private bool isPlatformActivated = false;

        private void Update()
        {
            if (isPlatformActivated) MoveTowardsWaypoint();
            else CheckForPlayerPresence();
        }

        private void MoveTowardsWaypoint()
        {
            platformObject.transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, movementSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

                if (currentWaypointIndex == 0 && isPlatformReachedEnd())
                {
                    SwitchBackToStartingPosition();
                }
            }
        }

        private void CheckForPlayerPresence()
        {
            Collider[] colliders = Physics.OverlapBox(transform.position, platformObject.GetComponent<Collider>().bounds.size * 0.5f, platformObject.transform.rotation);
            currentPlayersOnPlatform = 0;
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    currentPlayersOnPlatform++;
                    if (currentPlayersOnPlatform == maxCapacity || (maxCapacity == 1 && currentPlayersOnPlatform == 1))
                    {
                        isPlatformActivated = true;
                        break;
                    }
                }
            }
        }

        private bool isPlatformReachedEnd()
        {
            return currentWaypointIndex == 0;
        }

        private void SwitchBackToStartingPosition()
        {
            platformObject.transform.position = Vector3.MoveTowards(transform.position, waypoints[0].position, movementSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, waypoints[0].position) < 0.1f)
            {
                currentWaypointIndex = 0;
                isPlatformActivated = false;
            }
        }
    }
}
