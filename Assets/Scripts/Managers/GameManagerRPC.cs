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

        // Check if either of the player dies
        foreach (var player in players)
        {
            // Get the player component and check if the player is dead
            if (player.GetComponent<Player>().IsDead)
            {
                // Call RespawnPlayerServerRpc to respawn the player
                RespawnPlayerServerRpc(player.GetComponent<NetworkObject>().OwnerClientId);
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
            foreach (var player in players)
            {
                TakeDamageFromHeatWaveRpc(player.GetComponent<NetworkObject>().OwnerClientId);
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
            if (SetCheckpoint.position == null)
            {
                player.transform.position = originalSpawnPoint.position;
            }
            else
            {
                player.transform.position = SetCheckpoint.position;
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
}