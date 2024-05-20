using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class CollisionHandler : NetworkBehaviour
    {
        private GameManager gameManager;
        private HealthSystem health;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
        }

        public override void OnNetworkSpawn()
        {
            gameManager = GameManager.Instance;
        }

        #region Collision

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Collectible"))
            {
                if (!IsOwner) return;
                gameManager.UpdateCollectionCount();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Aqua Totem"))
            {
                if (!IsOwner) return;
                health.ApplyHealOverTime();
                HealOtherPlayerNearby();
            }
        }
        #endregion

        #region Custom Methods
        private void HealOtherPlayerNearby()
        {
            if (!IsOwner) return;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player == gameObject) continue;

                HealthSystem otherHealth = player.GetComponent<HealthSystem>();
                if (otherHealth != null)
                {
                    otherHealth.ApplyHealOverTime();
                }
            }
        }
        #endregion
    }
}
