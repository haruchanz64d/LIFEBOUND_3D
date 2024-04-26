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

    public NetworkVariable<Transform> lastCheckpointInteracted = new NetworkVariable<Transform>(null);
    
    public Transform originalSpawnpoint;

    void Awake()
    {
        originalSpawnpoint = GameObject.Find("Spawnpoint").transform;
    }
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (!IsServer)
        {
            lastCheckpointInteracted.Value = checkpoint;

            SetCheckpointServerRpc(checkpoint);
        }
        else
        {
            lastCheckpointInteracted.Value = checkpoint;

            SetCheckpointServerRpc(checkpoint);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCheckpointServerRpc(Transform checkpoint)
    {
        lastCheckpointInteracted.Value = checkpoint;
    }

    public Vector3 GetRespawnPosition()
    {
        if (lastCheckpointInteracted.Value != null)
        {
            return lastCheckpointInteracted.Value.position;
        }
        else
        {
            return originalSpawnpoint.position;
        }
    }
}