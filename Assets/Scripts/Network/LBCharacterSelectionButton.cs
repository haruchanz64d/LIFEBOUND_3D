using UnityEngine;
using Unity.Netcode;

public class LBCharacterSelectionButton : NetworkBehaviour
{
    public LBCharacterSelector characterSelector;

    public void OnSolSelected()
    {
        characterSelector.GetComponent<LBCharacterSelector>().OnCharacterSelectedSolServerRpc();
    }

    public void OnLunaSelected()
    {
        characterSelector.GetComponent<LBCharacterSelector>().OnCharacterSelectedLunaServerRpc();
    }
}
