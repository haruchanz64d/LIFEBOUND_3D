using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Manages the game logic and mechanics for the multiplayer game.
/// </summary>
public class GameManagerRPC : NetworkBehaviour
{
    public static GameManagerRPC Instance { get; private set; }

    public Transform lastCheckpointInteracted;

    public Transform originalSpawnpoint;

    void Awake()
    {
        originalSpawnpoint = GameObject.Find("Spawn Point").transform;
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public void SetCheckpoint(NetworkObject checkpoint)
    {
        if (!IsServer)
        {
            // Client-side prediction
            lastCheckpointInteracted = checkpoint.transform;

            // Notify server
            
        }
        else
        {
            // Server-side validation
            lastCheckpointInteracted = checkpoint.transform;

            // Notify clients
            
        }
    }

    public Vector3 GetRespawnPosition()
    {
        if (lastCheckpointInteracted != null)
        {
            return lastCheckpointInteracted.transform.position;
        }
        else
        {
            return originalSpawnpoint.position;
        }
    }
}