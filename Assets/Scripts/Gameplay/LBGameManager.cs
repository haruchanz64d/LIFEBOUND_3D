using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
using System.Collections.Generic;
public class LBGameManager : NetworkBehaviour
{
    public static LBGameManager Instance { get; private set; }

    [Header("Checkpoint System")]
    [SerializeField] private List<NetworkObject> players;
    [SerializeField] private Vector3 originalSpawnpoint;
    [SerializeField] private Vector3 lastCheckpointInteracted;
    public Vector3 SetLastCheckpointInteracted { set => lastCheckpointInteracted = value; }
    [Space]
    [Header("Heatwave")]
    private float heatwaveDamage = 2f;
    private float heatwaveInterval = 10f;
    private float heatwaveTimer = 0f;
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    private void Awake()
    {
        players = new List<NetworkObject>();
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            players.Add(client.PlayerObject);
        }
    }

    #region Player Death Events
    [ServerRpc(RequireOwnership = false)]
    public void SetLastCheckpointServerRpc(Vector3 checkpointPosition)
    {
        lastCheckpointInteracted = checkpointPosition;
        Debug.Log($"Checkpoint activated at X: {checkpointPosition.x}, Y: {checkpointPosition.y}, Z: {checkpointPosition.z}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDiedServerRpc(ulong clientId)
    {
        NetworkObject player = players.Find(player => player.OwnerClientId == clientId);
        if (player)
        {
            player.GetComponent<Player>().isDead = true;

            Vector3 spawnPoint = player.GetComponent<Player>().isDead ? lastCheckpointInteracted : originalSpawnpoint;
            if (lastCheckpointInteracted != spawnPoint)
            {
                lastCheckpointInteracted = spawnPoint;
            }
            RespawnPlayerServerRpc(player.OwnerClientId, spawnPoint);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RespawnPlayerServerRpc(ulong clientId, Vector3 spawnPoint)
    {
        NetworkObject player = players.Find(player => player.OwnerClientId == clientId);
        if (player)
        {
            player.transform.position = spawnPoint;
            player.GetComponent<Player>().isDead = false;
        }
    }
    #endregion

    #region Heatwave Events
    private void Update()
    {
        if (IsServer)
        {
            heatwaveTimer += Time.deltaTime;
            if (heatwaveTimer >= heatwaveInterval)
            {
                heatwaveTimer = 0f;
                ApplyHeatwaveDamageServerRpc();
                Debug.Log(Equals(heatwaveTimer, 0f) ? "Heatwave damage applied." : "Heatwave damage not applied.");
            }

            foreach(NetworkObject player in players)
            {
                if (player)
                {
                    if (player.transform.position.y < -10f)
                    {
                        RespawnPlayerServerRpc(player.OwnerClientId, originalSpawnpoint);
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyHeatwaveDamageServerRpc()
    {
        foreach (NetworkObject player in players)
        {
            if (player)
            {
                player.GetComponent<Player>().TakeDamage(heatwaveDamage);
            }
        }
    }
    #endregion
}