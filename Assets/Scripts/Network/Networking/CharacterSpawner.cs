using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LBCharacterDatabase characterDatabase;
    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        foreach (var client in HostManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPoint.transform.position, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}
