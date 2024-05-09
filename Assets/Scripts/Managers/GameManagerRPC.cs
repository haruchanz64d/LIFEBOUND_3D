using UnityEngine;
using Unity.Netcode;
using LB.Character;

public enum GameState
{
    Alive,
    Dead,
    SoulSwapping
}

public class GameManagerRPC : NetworkBehaviour
{
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.Alive);
    public static GameManagerRPC Instance { get; private set; }

    [Header("Checkpoint System")]
    public Transform lastCheckpointInteracted;

    public Transform originalSpawnpoint;

    [Header("Death System")]
    public bool isDead;

    [Header("Soul Swap System")]
    public float soulSwapCooldown = 30f;

    [Header("Heat Wave")]
    public bool isHeatWaveActive = false;
    private int heatWaveDmg = 2;
    private float heatTickInterval = 10f;
    [SerializeField] private float heatWaveTimer = 0f;

    void Awake()
    {
        originalSpawnpoint = GameObject.Find("Spawn Point").transform;
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;
        isHeatWaveActive = true;
    }

    private void Update()
    {
        HandleDoT();
    }

    public void HandleDoT()
    {
        if (isHeatWaveActive)
        {
            heatWaveTimer += Time.deltaTime;
            if (heatWaveTimer >= heatTickInterval)
            {
                ApplyHeatwaveDamageServerRpc();
                heatWaveTimer = 0f;
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
        if (isDead)
        {
            gameState.Value = GameState.Dead;
        }
        else
        {
            gameState.Value = GameState.Alive;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSoulSwapServerRpc(bool isSoulSwapEnabled)
    {
        if (gameState.Value == GameState.SoulSwapping) return;
        gameState.Value = GameState.SoulSwapping;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyHeatwaveDamageServerRpc()
    {
        foreach (NetworkClient player in NetworkManager.Singleton.ConnectedClientsList)
        {
            player.PlayerObject.GetComponent<Player>().UpdateHealth(player.PlayerObject.GetComponent<Player>().currentHealth - heatWaveDmg);
            Debug.Log("Player took damage from DoT");
        }
    }
}