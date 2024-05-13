using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Camera
{
    public class ResetCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLookCamera;
        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                freeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            }
            else
            {
                freeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            }
        }
    }
}