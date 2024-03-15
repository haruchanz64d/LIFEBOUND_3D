/// <summary>
/// Manages character selection and spawning for networked players.
/// </summary>
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class LBCharacterSelector : NetworkBehaviour
{
    /// <summary>
    /// List of character prefabs to choose from.
    /// </summary>
    [SerializeField] private List<GameObject> characters = new List<GameObject>();

    /// <summary>
    /// Reference to the character selection UI.
    /// </summary>
    [SerializeField] private GameObject characterSelection;

    /// <summary>
    /// Handles behavior when the networked object is spawned.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) characterSelection.SetActive(false);
    }

    /// <summary>
    /// Spawns the "Sol" character.
    /// </summary>
    public void SpawnSol()
    {
        SpawnServerRpc(0, true);
        characterSelection.SetActive(false);
    }

    /// <summary>
    /// Spawns the "Luna" character.
    /// </summary>
    public void SpawnLuna()
    {
        SpawnServerRpc(1, true);
        characterSelection.SetActive(false);
    }

    /// <summary>
    /// Spawns the selected character based on the character index.
    /// </summary>
    /// <param name="characterIndex">The index of the character to spawn.</param>
    /// <param name="isLocalPlayer">Flag indicating if the player is local.</param>
    [ServerRpc]
    public void SpawnServerRpc(int characterIndex, bool isLocalPlayer)
    {
        GameObject player = Instantiate(characters[characterIndex], GameObject.FindGameObjectWithTag("Spawn Point").transform.position, Quaternion.identity);
        isLocalPlayer = player.GetComponent<NetworkObject>().IsLocalPlayer;
        // idk what to put here for another fix
    }

}
