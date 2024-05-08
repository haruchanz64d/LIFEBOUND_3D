using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.PlayerLoop;
/// <summary>
/// Manages the game logic and mechanics for the multiplayer game.
/// </summary>
public class GameManagerRPC : NetworkBehaviour
{
    public static GameManagerRPC Instance { get; private set; }

    [Header("Checkpoint System")]
    public Transform lastCheckpointInteracted;

    public Transform originalSpawnpoint;

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
        }
    }
     [Header("DoT")]
    public bool isDotActive = false;
    private int dotDamage = 2;
    private float dotTickInterval = 10f;
    [SerializeField] private float dotTimer = 0f;

    void Awake()
    {
        originalSpawnpoint = GameObject.Find("Spawn Point").transform;
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;
        isDotActive = true;
    }

    private void Update()
    {
        if(isDotActive){
            dotTimer += Time.deltaTime;
            if(dotTimer >= dotTickInterval){
                ApplyBurningCoroutineServerRpc();
                dotTimer = 0f;
            }
        }
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

    [ServerRpc(RequireOwnership = false)]
    private void ApplyBurningCoroutineServerRpc()
    {
        foreach(var players in NetworkManager.Singleton.ConnectedClientsList)
        {
            if(players.PlayerObject != null)
            {
                var player = players.PlayerObject.GetComponent<Player>();
                player.TakeDamage(dotDamage);
            }
        }
    }
}