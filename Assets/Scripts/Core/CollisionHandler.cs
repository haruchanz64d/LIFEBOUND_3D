// CollisionHandler.cs
using LB.Environment.Objects;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class CollisionHandler : NetworkBehaviour
    {
        private GameManager gameManagerRPC;
        private HealthSystem health;
        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            gameManagerRPC = FindObjectOfType<GameManager>();
        }

        #region Collision
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                if (!IsOwner) return;
                other.gameObject.GetComponent<Checkpoint>().OnCheckpointActivated();
                GameManager.Instance.LastInteractedCheckpointPosition = other.gameObject.GetComponent<Checkpoint>().GetCheckpointPosition().transform.position;
            }
            if (other.CompareTag("Platform"))
            {
                // TODO: Parenting issue on the client side.
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
        #endregion

        #region RPC Functions

        #endregion
    }
}