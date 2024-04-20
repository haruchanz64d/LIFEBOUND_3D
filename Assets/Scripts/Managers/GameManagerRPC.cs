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
        /*if (!IsServer) { return; }
        if(players.GetEnumerator().MoveNext())
        {
            foreach (var player in players)
            {
                if (player.GetComponent<Player>().IsDead)
                {
                    player.GetComponent<Player>().RespawnPlayer();
                }
            }
        }*/
    }
}