using UnityEngine;
using Unity.Netcode;
using TMPro;
using LB.Character;
public class LBGameManager : NetworkBehaviour
{
    public static LBGameManager Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public float GetHeatTimer()
    {
        return 120f;
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastPlayerDeathServerRpc(ulong playerId)
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.ClientId != playerId)
            {
                player.PlayerObject.GetComponent<Player>().Die();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastPlayerPositionServerRpc(Vector3 position)
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            player.PlayerObject.GetComponent<Player>().GetCurrentPosition(position);

            //Debug.Log("Broadcasting players position to all clients: " + position);
        }
    }
}
