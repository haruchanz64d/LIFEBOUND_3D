using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LBCheckpoint : MonoBehaviour
{
    [SerializeField] private GameObject unactivatedCheckpoint;
    [SerializeField] private GameObject activatedCheckpoint;
    [SerializeField] private Transform checkpointPosition;
    public Transform GetCheckpointPosition() { return checkpointPosition; }

    private void Awake()
    {
        transform.position = checkpointPosition.position;
        OnCheckpointDeactivated();
    }
    public void OnCheckpointActivated()
    {
        unactivatedCheckpoint.SetActive(false);
        activatedCheckpoint.SetActive(true);
    }

    public void OnCheckpointDeactivated()
    {
        unactivatedCheckpoint.SetActive(true);
        activatedCheckpoint.SetActive(false);
    }
}
