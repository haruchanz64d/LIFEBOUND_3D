using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LB.Character.Interaction;

namespace LB.Environment.Objects
{
    public class LBCheckpoint : MonoBehaviour
    {
        [SerializeField] private LBInteractionManager interactionManager;
        private Transform checkpointPosition;

        private void LateUpdate()
        {
            interactionManager = GameObject.FindGameObjectWithTag("Player").GetComponent<LBInteractionManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                interactionManager.ShowInteractionPanel();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                interactionManager.HideInteractionPanel();
            }
        }

        public void OnCheckpointActivated()
        {

        }
    }
}
