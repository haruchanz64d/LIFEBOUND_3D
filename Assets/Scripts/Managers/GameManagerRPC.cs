using UnityEngine;
using Unity.Netcode;


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
            Debug.Log("Respawned at checkpoint: " + lastCheckpointInteracted.name);
            return lastCheckpointInteracted.transform.position;
        }
        else
        {
            Debug.Log("Respawned at original spawnpoint");
            return originalSpawnpoint.transform.position;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerDieServerRpc(bool isDead)
    {
       
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSoulSwapServerRpc(bool isSoulSwapEnabled)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyHeatwaveDamageServerRpc()
    {
        
    }
}