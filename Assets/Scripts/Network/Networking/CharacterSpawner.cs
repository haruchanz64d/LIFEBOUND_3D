using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform lookPoint;
    [SerializeField] private LBCharacterDatabase characterDatabase;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        foreach (var client in HostManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                Vector3 lookPosition = lookPoint.position;
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPoint.position, Quaternion.LookRotation(lookPosition));
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}
