using UnityEngine;
using Unity.Netcode;
using LB.Character;
public class LBSoulSwapPad : NetworkBehaviour
{
    [SerializeField] private GameObject pairedSoulSwapPad;
    private bool isPlayerStandingOnPad;
    private NetworkObject playerObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerStandingOnPad = true;
            playerObject = other.gameObject.GetComponent<NetworkObject>();
            CheckSoulSwap();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerObject = null;
            isPlayerStandingOnPad = false;
            CheckSoulSwap();
        }
    }

    private void CheckSoulSwap()
    {
        if (isPlayerStandingOnPad && pairedSoulSwapPad.GetComponent<LBSoulSwapPad>().isPlayerStandingOnPad)
        {
            if (playerObject.GetComponent<Player>().IsSoulSwapSkillActivated)
            {
                SoulSwapClientRpc(playerObject.NetworkObjectId);
            }
        }
    }

    [ClientRpc]
    private void SoulSwapClientRpc(ulong playerNetworkObjectId)
    {
        if (IsLocalPlayer)
        {
            transform.position = pairedSoulSwapPad.transform.position;
        }
        else if (playerNetworkObjectId != 0)
        {
            NetworkObject player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
            player.transform.position = transform.position;
        }
    }
}
