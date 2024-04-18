using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
using System.Collections.Generic;
using System.Linq;
public class LBGameManager : NetworkBehaviour
{
    public static LBGameManager Instance { get; private set; }
    private GameObject[] players;
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        players.Aggregate(new List<Player>(), (list, player) =>
        {
            list.Add(player.GetComponent<Player>());
            return list;
        });
    }

    [ClientRpc]
    public void RespawnPlayerClientRpc()
    {
        players.ToList().ForEach(player => player.GetComponent<Player>().Respawn());
    }
}