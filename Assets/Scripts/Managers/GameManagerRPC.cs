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

    public NetworkVariable<NetworkObject> lastCheckpointInteracted = new NetworkVariable<NetworkObject>(null);

    public Transform originalSpawnpoint;

    void Awake()
    {
        originalSpawnpoint = GameObject.Find("Spawnpoint").transform;
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
            lastCheckpointInteracted.Value = checkpoint;

            // Notify server
            SetCheckpointServerRpc(checkpoint);
        }
        else
        {
            // Server-side validation
            lastCheckpointInteracted.Value = checkpoint;

            // Notify clients
            SetCheckpointClientRpc(checkpoint);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCheckpointServerRpc(NetworkObject checkpoint)
    {
        lastCheckpointInteracted.Value = checkpoint;
    }

    [ClientRpc]
    public void SetCheckpointClientRpc(NetworkObject checkpoint)
    {
        lastCheckpointInteracted.Value = checkpoint;
    }

    public Vector3 GetRespawnPosition()
    {
        if (lastCheckpointInteracted.Value != null)
        {
            return lastCheckpointInteracted.Value.transform.position;
        }
        else
        {
            return originalSpawnpoint.position;
        }
    }
}