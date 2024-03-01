using Unity.Netcode;
using UnityEngine;

public class LBPlayerPrefabSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject LUNA_PLAYER;
    [SerializeField] private GameObject SOL_PLAYER;

    private void Awake()
    {
        if (IsHost) Instantiate(SOL_PLAYER);
        else Instantiate(LUNA_PLAYER);
    }
}
