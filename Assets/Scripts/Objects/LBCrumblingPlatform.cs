/// <summary>
/// This script implements a crumbling platform that destroys itself after a delay, triggered by player presence.
///
/// Key features:
/// - Requires a specific number of players to stand on it to prevent destruction (configurable).
/// - Crumbles after a set delay if the trigger condition is met.
/// - Optionally destroys even if players leave the platform before the delay ends.
/// - Provides flexibility for customization:
///     - Set the required player count (`requiredPlayersToStay`).
///     - Adjust the crumble delay (`crumbleDelay`).
///     - Enable early crumble on player exit (`destroyOnTriggerExit`).
/// </summary>

using UnityEngine;

namespace LB.Environment.Objects
{
    public class LBCrumblingPlatform : MonoBehaviour
    {
        [SerializeField] private int requiredPlayersToStay = 0;
        [SerializeField] private float crumbleDelay = 3f;
        [SerializeField] private bool destroyOnTriggerExit = false;
        [SerializeField] private GameObject platformObject;

        private bool isCrumbling = false;
        private bool hasTriggered = false;
        private float timer;

        private void Update()
        {
            if (isCrumbling)
            {
                timer += Time.deltaTime;
                if (timer >= crumbleDelay)
                {
                    DestroyPlatform();
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (requiredPlayersToStay == 0 ||
                    (requiredPlayersToStay > 0 && CountPlayersOnPlatform() == requiredPlayersToStay))
                {
                    TriggerCrumble();
                }
            }
        }
    

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag("Player") && destroyOnTriggerExit)
            {
                TriggerCrumble();
            }
        }

        private void TriggerCrumble()
        {
            if (!hasTriggered)
            {
                hasTriggered = true;
                isCrumbling = true;
                timer = 0f;
            }
        }

        private void DestroyPlatform()
        {
            Destroy(platformObject);
        }

        private int CountPlayersOnPlatform()
        {
            return 0;
        }
    }
}
