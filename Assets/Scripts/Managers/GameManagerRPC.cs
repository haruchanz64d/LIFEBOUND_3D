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
    [Header("Checkpoint Mechanic")]
    private Transform checkpointActivated;
    private Transform originalSpawnPoint;
    public Transform SetCheckpoint
    {
        get => checkpointActivated;
        set => checkpointActivated = value;
    }
    [Space]
    [Header("Player Collection")]
    private GameObject[] players;
    [Space]
    [Header("Heat Wave Mechanic")]
    private float heatWaveDamage = 2f;
    private float timeBetweenDamage = 8f;
    [Space]
    [Header("Soul Swap Mechanic")]
    private float soulSwapDuration = 10f;
    private float soulSwapCooldown = 30f;
    private float soulSwapTimer;
    private bool isSoulSwapping;
    public bool IsSoulSwapping => isSoulSwapping;
    public bool IsSoulSwapEnabled
    {
        get
        {
            if (Time.time > soulSwapTimer + soulSwapCooldown)
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Called when the network object is spawned.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        originalSpawnPoint = GameObject.Find("Spawn Point").transform;
        SetCheckpoint = originalSpawnPoint;

        players = GameObject.FindGameObjectsWithTag("Player");

        Debug.Log($"Player count: {NetworkManager.Singleton.ConnectedClientsList.Count}");
    }

    /// <summary>
    /// Update is called every frame.
    /// </summary>
    private void Update()
    {
        // Handle the heat wave event
        HandleHeatWaveRpc();

        // Handle soul swap cooldown
        if (Time.time > soulSwapTimer + soulSwapCooldown)
        {
            foreach (var player in players)
            {
                player.GetComponent<Player>().IsSoulSwapping = true;
            }
        }
    }
    #region Heatwave Mechanic
    /// <summary>
    /// Handles the heat wave event, causing damage to all players within a certain time interval.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void HandleHeatWaveRpc()
    {
        if (Time.time > timeBetweenDamage)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                TakeDamageFromHeatWaveRpc(client.ClientId);
            }
            timeBetweenDamage = Time.time + 8f;
        }
    }

    /// <summary>
    /// Takes damage from the heat wave event.
    /// </summary>
    /// <param name="clientId"></param>
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TakeDamageFromHeatWaveRpc(ulong clientId)
    {
        var player = players.FirstOrDefault(x => x.GetComponent<NetworkObject>().OwnerClientId == clientId);
        if (player != null)
        {
            player.GetComponent<Player>().TakeDamage(heatWaveDamage);
        }
    }
    #endregion


    #region Respawn Mechanic
    /// <summary>
    /// Respawns the player at the original spawn point or the checkpoint.
    /// </summary>
    /// <param name="clientId"></param>
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RespawnPlayerServerRpc(ulong clientId)
    {
        var player = players.FirstOrDefault(x => x.GetComponent<NetworkObject>().OwnerClientId == clientId);
        if (player != null)
        {
            if (SetCheckpoint == null)
            {
                player.transform.position = originalSpawnPoint.position;
            }
            else
            {
                player.transform.position = SetCheckpoint.position;
            }
            player.GetComponent<Player>().TakeDamage(100f); // Set HP to zero
            player.GetComponent<Player>().IsDead = true;
            StartCoroutine(player.GetComponent<Player>().AnimateBeforeRespawn()); // Play death animation and respawn
                                                                                  // Respawn the other player
            foreach (var p in players)
            {
                if (p != player)
                {
                    p.transform.position = player.transform.position; // Respawn to the same position
                    p.GetComponent<Player>().TakeDamage(100f); // Set HP to zero
                    p.GetComponent<Player>().IsDead = true;
                    StartCoroutine(p.GetComponent<Player>().AnimateBeforeRespawn()); // Play death animation and respawn
                }
            }
        }
    }

    /// <summary>
    /// Respawns the player and the other player to the last checkpoint.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RespawnBothPlayersRpc(ulong clientId)
    {
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                RespawnPlayerServerRpc(clientId);
            }
            else
            {
                RespawnPlayerServerRpc(player.GetComponent<NetworkObject>().OwnerClientId);
            }
        }
    }


    #endregion

    #region Soul Swap Mechanic
    /// <summary>
    /// Activates the soul swap mechanic.
    /// </summary>
    /// <param name="isSoulSwapping"></param>
    /// <param name="clientId"></param>
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void IsSoulSwapEnabledRpc(bool isSoulSwapping, ulong clientId)
    {
        var player = players.FirstOrDefault(x => x.GetComponent<NetworkObject>().OwnerClientId == clientId);
        if (player != null)
        {
            player.GetComponentInChildren<Player>().IsSoulSwapping = isSoulSwapping;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StartSoulSwapCooldownRpc()
    {
        soulSwapTimer = Time.time;
        foreach (var player in players)
        {
            player.GetComponent<Player>().IsSoulSwapping = true;
        }
    }

    /// <summary>
    /// Sets the soul swap timer.
    /// </summary>
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SoulSwapTimerRpc()
    {
        foreach (var player in players)
        {
            player.GetComponent<Player>().SoulSwapTimer();
        }
    }
    #endregion

    #region Lava Damage
    public void ShareLavaDamageRpc(ulong clientId)
    {
        var player = players.FirstOrDefault(x => x.GetComponent<NetworkObject>().OwnerClientId == clientId);
        if (player != null)
        {
            player.GetComponent<Player>().TakeDamage(1f);
        }
    }
    #endregion
}