using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CollectibleSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject collectiblePrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsServer)
        {
            SpawnCollectibles();
        }
    }

    private void SpawnCollectibles()
    {
        GameObject collectibleInstance = Instantiate(collectiblePrefab);

        NetworkObject networkObject = collectibleInstance.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }
}
