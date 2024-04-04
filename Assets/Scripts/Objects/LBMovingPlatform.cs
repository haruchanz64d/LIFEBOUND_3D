using UnityEngine;

namespace LB.Environment.Objects
{
    public class LBMovingPlatform : MonoBehaviour
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

        private bool isPlatformReachedEnd()
        {
            return currentWaypointIndex == 0;
        }

        private void SwitchBackToStartingPosition()
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[0].position, movementSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, waypoints[0].position) < 0.1f)
            {
                currentWaypointIndex = 0;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.SetParent(transform);
                UpdatePlayerPosition();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.transform.SetParent(null);
            }
        }

        private void UpdatePlayerPosition()
        {
            Vector3 platformDelta = transform.position - lastPlatformPosition;
            if (platformDelta != Vector3.zero)
            {
                CharacterController playerController = GetComponent<CharacterController>();
                if (playerController != null)
                {
                    playerController.Move(platformDelta);
                }
            }
            lastPlatformPosition = transform.position;
        }
    }
}
