using UnityEngine;
using Unity.Netcode;

public class LBCharacterSelector : NetworkBehaviour
{
    [SerializeField] private NetworkObject solPrefab;
    [SerializeField] private NetworkObject lunaPrefab;

    [SerializeField] private GameObject characterSelectionCanvas;

    [ServerRpc (RequireOwnership = false)]
    public void OnCharacterSelectedSolServerRpc()
    {
        var sol = Instantiate(solPrefab);
        sol.Spawn();
        characterSelectionCanvas.SetActive(false);
    }

    [ServerRpc (RequireOwnership = false)]
    public void OnCharacterSelectedLunaServerRpc()
    {
        var luna = Instantiate(lunaPrefab);
        luna.Spawn();
        characterSelectionCanvas.SetActive(false);
    }
}
