using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private LBCharacterDatabase characterDatabase;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        foreach (var client in HostManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                var randomSpawnIndex = Random.Range(0, spawnPoints.Length);
                var spawnPoint = spawnPoints[randomSpawnIndex];
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPoint.position, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}
