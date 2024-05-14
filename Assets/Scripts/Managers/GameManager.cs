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
    private float heatWaveTimer = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isHeatWaveActivated = true;
    }
    private void Awake()
    {
        if (!IsServer) return;
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

    #region RPCs (Heat Wave)
    [ServerRpc(RequireOwnership = false)]
    private void ApplyHeatwaveDamageServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if(player.TryGetComponent(out HealthSystem health))
            {
                ApplyHeatwaveDamageClientRpc(heatWaveDamage);
            }
        }
    }

    [ClientRpc]
    public void ApplyHeatwaveDamageClientRpc(int damage)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(damage);
            }
        }
    }

    #endregion
}