using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
using System.Collections.Generic;
using System.Linq;
public class GameManagerRPC : NetworkBehaviour
{
    public static GameManagerRPC Instance { get; private set; }
    private Transform checkpointActivated;
    private Transform originalSpawnPoint;
    private GameObject[] players;
    public Transform SetCheckpoint
    {
        get => checkpointActivated;
        set => checkpointActivated = value;
    }
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    private void Awake()
    {
        originalSpawnPoint = GameObject.Find("Spawn Point").transform;
        SetCheckpoint = originalSpawnPoint;

        players = GameObject.FindGameObjectsWithTag("Player");
    }

    private void Update()
    {
        
    }

    [Rpc(SendTo.Everyone)]
    public void RespawnPlayerServerRpc(ulong clientId)
    {
        var player = players.FirstOrDefault(x => x.GetComponent<NetworkObject>().OwnerClientId == clientId);
        if (player != null)
        {
            player.transform.position = SetCheckpoint.position;
        }
    }
}