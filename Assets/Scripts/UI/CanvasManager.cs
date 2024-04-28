using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CanvasManager : NetworkBehaviour
{
    [SerializeField] private Canvas playerCanvasPrefab;
    private Canvas playerCanvasInstance;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if (IsOwner)
        {
            playerCanvasInstance = Instantiate(playerCanvasPrefab, transform);
            playerCanvasInstance.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(IsOwner)
        {
            playerCanvasInstance.transform.position = transform.position;
        }
    }
}
