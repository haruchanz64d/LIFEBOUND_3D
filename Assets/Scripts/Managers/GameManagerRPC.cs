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
    public int currentHealth;

    [Header("Death System")]
    public bool isDead;

    [Header("Soul Swap System")]
    public float soulSwapCooldown = 30f;
    private bool isSoulSwapEnabled;

    public bool IsSoulSwapEnabled
    {
        get => isSoulSwapEnabled;
        set
        {
            isSoulSwapEnabled = value;
            if (isSoulSwapEnabled)
            {
                soulSwapCooldown = 0;
            }
        }
    }
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
        lastCheckpointInteracted = checkpoint.transform;

        if (lastCheckpointInteracted != null)
        {
            Debug.Log("Checkpoint set to: " + lastCheckpointInteracted.name);
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
        this.isDead = isDead;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSoulSwapCooldownServerRpc(float cooldown)
    {
        soulSwapCooldown = cooldown;
    }
}