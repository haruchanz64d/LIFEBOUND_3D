using Assets.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameObject unactivatedCheckpoint;
    [SerializeField] private GameObject activatedCheckpoint;
    [SerializeField] private Transform checkpointPosition;
    [SerializeField] private AudioClip checkpointActivated;

    private void Awake()
    {
        unactivatedCheckpoint.SetActive(true);
        activatedCheckpoint.SetActive(false);
    }
    public void OnCheckpointActivated()
    {
        unactivatedCheckpoint.SetActive(false);
        activatedCheckpoint.SetActive(true);
        AudioManager.Instance.PlaySound(checkpointActivated);
    }

    public Vector3 SetCheckpointPosition()
    {
        Debug.Log($"Checkpoint position set to: {checkpointPosition.position}");
        return checkpointPosition.position;
    }
}
