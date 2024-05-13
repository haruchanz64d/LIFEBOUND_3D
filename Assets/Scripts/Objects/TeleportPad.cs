using UnityEngine;
using Unity.Netcode;
using LB.Character;
public class TeleportPad : MonoBehaviour
{
    [SerializeField] private Transform pairedSoulSwapPadTransform;
    public Transform PairedSoulSwapPadTransform => pairedSoulSwapPadTransform;


    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerToCheckpointServerRpc()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (pairedSoulSwapPadTransform == null) return;

        var player = GetComponent<NetworkObject>();
        player.transform.position = pairedSoulSwapPadTransform.position;
    }
}
    