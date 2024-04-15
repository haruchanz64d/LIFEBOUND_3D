using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
using System.Collections.Generic;
using UnityEditor.PackageManager;
public class LBGameManager : NetworkBehaviour
{
    public static LBGameManager Instance { get; private set; }

    [Header("Checkpoint System")]
    [SerializeField] private List<Player> players;
    [SerializeField] private Vector3 originalSpawnpoint;
    [SerializeField] private Vector3 lastCheckpointInteracted;
    public Vector3 SetLastCheckpointInteracted { set => lastCheckpointInteracted = value; }
    [Space]
    [Header("Heatwave")]
    private float heatwaveDamage = 2f;
    private float heatwaveInterval = 10f;
    private float heatwaveTimer = 0f;
    [Space]
    [Header("Soul Swap Skill System")]
    private Dictionary<ulong, Transform> connectedClientsTransform = new Dictionary<ulong, Transform>();
    public override void OnNetworkSpawn()
    {
        Instance = this;

        players = new List<Player>();
        players.Add(players.Find(p => p.IsLocalPlayer));

        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            connectedClientsTransform.Add(client.ClientId, client.PlayerObject.transform);
        }
    }

    #region Player Death Events
    [ServerRpc(RequireOwnership = false)]
    public void SetLastCheckpointServerRpc(Vector3 checkpointPosition)
    {
        lastCheckpointInteracted = checkpointPosition;
        Debug.Log($"Checkpoint activated at X: {checkpointPosition.x}, Y: {checkpointPosition.y}, Z: {checkpointPosition.z}");
    }

    [ServerRpc]
    public void PlayerDiedServerRpc(ulong clientId)
    {
        Player player = players.Find(p => p.OwnerClientId == clientId);
        if (player)
        {
            player.isDead = true;

            Vector3 spawnPoint = player.isDead ? lastCheckpointInteracted : originalSpawnpoint;
            if (lastCheckpointInteracted != spawnPoint)
            {
                lastCheckpointInteracted = spawnPoint;
            }
            PlayerDeathClientRpc(player.OwnerClientId, spawnPoint);
        }
    }

    [ClientRpc]
    private void PlayerDeathClientRpc(ulong clientId, Vector3 spawnPoint)
    {
        Player player = players.Find(p => p.OwnerClientId == clientId);
        if (player)
        {
            player.transform.position = spawnPoint;
            player.isDead = false;
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
                ApplyHeatwaveDamage();
            }

            foreach (Player player in players)
            {
                if (player && player.GetCurrentHP <= 0)
                {
                    PlayerDiedServerRpc(player.OwnerClientId);
                }
            }
        }
    }


    private void ApplyHeatwaveDamage()
    {
        foreach (Player player in players)
        {
            if (player)
            {
                player.TakeDamage(heatwaveDamage);
            }
        }
    }
    #endregion
}