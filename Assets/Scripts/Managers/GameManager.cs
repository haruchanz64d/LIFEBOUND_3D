using UnityEngine;
using Unity.Netcode;
using LB.Character;
using Assets.Scripts.Core;
using System;


public class GameManager: NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Checkpoint")]
    [SerializeField] private GameObject defaultSpawn;
    public GameObject DefaultSpawn => defaultSpawn;
    private Vector3 lastInteractedCheckpointPosition;
    public Vector3 LastInteractedCheckpointPosition
    {
        get => lastInteractedCheckpointPosition;
        set => lastInteractedCheckpointPosition = value;
    }

    [Header("Soul Swap")]
    private float soulSwapCooldown = 30f;
    public float SoulSwapCooldown => soulSwapCooldown;

    [Header("Heat Wave")]
    private bool isHeatWaveActivated;
    private int heatWaveDamage = 2;
    private float heatTickInterval = 10f;
    private float damageTickInterval;
    private float heatWaveTimer = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isHeatWaveActivated = true;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if(isHeatWaveActivated)
        {
            heatWaveTimer += Time.deltaTime;
            if(heatWaveTimer >= heatTickInterval)
            {
                ApplyHeatwaveDamageServerRpc();
                heatWaveTimer = 0f;
            }
        }
    }

    #region Server RPCs
    [ServerRpc(RequireOwnership = false)]
    private void ApplyHeatwaveDamageServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            if(player.TryGetComponent(out HealthSystem health))
            {
                ApplyHeatwaveDamageClientRpc(OwnerClientId, heatWaveDamage);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleSoulSwapCooldownServerRpc()
    {
        foreach (var players in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (players.PlayerObject.TryGetComponent(out SoulSwap swap))
            {
                if (swap.IsSoulSwapReady)
                {
                    swap.IsSoulSwapReady = false;
                    swap.SoulSwapCooldown = soulSwapCooldown;
                    SwapPlayerModelClientRpc();
                    PlaySoulSwapAnimationClientRpc();
                }
                else
                {
                    swap.SoulSwapCooldown -= Time.deltaTime;
                    if (swap.SoulSwapCooldown <= 0)
                    {
                        swap.IsSoulSwapReady = true;
                        ResetSoulSwapAnimationClientRpc();
                        ResetPlayerModelClientRpc();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckForDeathServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            HealthSystem health = player.GetComponent<HealthSystem>();
            if(health.IsPlayerDead)
            {
                health.StartRespawnTimer(player.GetComponent<NetworkObject>().OwnerClientId);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerToDefaultSpawnServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            player.transform.position = defaultSpawn.transform.position;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayersToCheckpointServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            player.transform.position = lastInteractedCheckpointPosition;
        }
    }
    #endregion

    #region Client RPCs
    [ClientRpc]
    public void ApplyHeatwaveDamageClientRpc(ulong playerNetworkId, int damage)
    {
        HealthSystem health = NetworkManager.Singleton.ConnectedClients[playerNetworkId].PlayerObject.GetComponent<HealthSystem>();
        health.TakeDamage(damage);
    }

    [ClientRpc]
    private void SwapPlayerModelClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(var player in players)
        {
            if(player.TryGetComponent(out SoulSwap swap))
            {
                swap.SwapPlayerModelClientRpc();
            }
        }
    }

    [ClientRpc]
    private void ResetPlayerModelClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(var player in players)
        {
            if(player.TryGetComponent(out SoulSwap swap))
            {
                swap.ResetPlayerModelClientRpc();
            }
        }
    }

    [ClientRpc]
    private void PlaySoulSwapAnimationClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.TryGetComponent(out SoulSwap swap))
            {
                swap.PlaySoulSwapAnimationClientRpc();
            }
        }
    }

    [ClientRpc]
    private void ResetSoulSwapAnimationClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.TryGetComponent(out SoulSwap swap))
            {
                swap.ResetSoulSwapAnimationClientRpc();
            }
        }
    }

    #endregion
}