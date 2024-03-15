using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class LBCharacterSelector : NetworkBehaviour
{
    [SerializeField] private List<GameObject> characters = new List<GameObject>();
    [SerializeField] private GameObject characterSelection;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) characterSelection.SetActive(false);
    }

    public void SpawnSol()
    {
        SpawnServerRpc(0, true);
        characterSelection.SetActive(false);
    }

    public void SpawnLuna()
    {
        SpawnServerRpc(1, true);
        characterSelection.SetActive(false);
    }

    [ServerRpc]
    public void SpawnServerRpc(int characterIndex, bool isLocalPlayer)
    {
        GameObject player = Instantiate(characters[characterIndex], GameObject.FindGameObjectWithTag("Spawn Point").transform.position, Quaternion.identity);
        isLocalPlayer = player.GetComponent<NetworkObject>().IsLocalPlayer;
        
    }

}
