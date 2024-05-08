using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.PlayerLoop;
public class LBCameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook virtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;
    private float m_ShakeIntensity = 5f;
    private float m_ShakeTime = 0.2f;
    private float m_Timer;

    private void Start()
    {
        StopShake();
    }
    
    private void Update()
    {
        if(m_Timer > 0)
        {
            m_Timer -= Time.deltaTime;
            if(m_Timer <= 0)
            {
                StopShake();
            }
        }
    }
    public void ShakeCamera()
    {
        virtualCameraNoise = virtualCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        virtualCameraNoise.m_AmplitudeGain = m_ShakeIntensity;
        m_Timer = m_ShakeTime;
    }

    public void StopShake()
    {
        virtualCameraNoise = virtualCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        virtualCameraNoise.m_AmplitudeGain = 0;
        m_Timer = 0;
    }
}
