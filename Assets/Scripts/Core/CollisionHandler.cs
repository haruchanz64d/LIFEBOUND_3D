// CollisionHandler.cs
using LB.Environment.Objects;
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
            if (other.CompareTag("Platform"))
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject == null) return;
                if (networkObject.OwnerClientId != OwnerClientId) return;
                if(networkObject.OwnerClientId == OwnerClientId)
                {
                    transform.SetParent(other.transform);
                }
            }
        }


        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Lava"))
            {
                if (!IsOwner) return;
                health.ApplyLavaDoT();
            }

            if (other.CompareTag("Aqua Totem"))
            {
                if (!IsOwner) return;
                health.ApplyHealOverTime();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Lava"))
            {
                if (!IsOwner) return;
                health.StopLavaDoT();
            }

            if (other.CompareTag("Platform"))
            {
                NetworkObject networkObject = other.GetComponent<NetworkObject>();
                if (networkObject == null) return;
                if (networkObject.OwnerClientId != OwnerClientId) return;
                if (networkObject.OwnerClientId == OwnerClientId)
                {
                    transform.SetParent(null);
                }
            }
        }
        #endregion

        #region RPC Functions

        #endregion
    }
}