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

    [Header("Checkpoint System")]
    public Transform lastCheckpointInteracted;

    public Transform originalSpawnpoint;

    [Header("Health System")]
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, (NetworkVariableReadPermission)NetworkVariableWritePermission.Owner);

    [Header("Death System")]
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, (NetworkVariableReadPermission)NetworkVariableWritePermission.Owner);

    [Header("Soul Swap System")]
    public NetworkVariable<float> soulSwapCooldown = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    [Header("Burning System")]
    public int burningDamage = 2;
    public float burningInterval = 10f;

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

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerDieServerRpc(bool isDead)
    {
        this.isDead.Value = isDead;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSoulSwapCooldownServerRpc(float cooldown)
    {
        soulSwapCooldown.Value = cooldown;
    }
}