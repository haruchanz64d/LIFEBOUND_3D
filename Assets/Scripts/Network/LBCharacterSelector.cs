using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class LBCharacterSelector : NetworkBehaviour
{
    [SerializeField] private GameObject solPrefab;
    [SerializeField] private GameObject lunaPrefab;
    [SerializeField] private int clientID;

    [SerializeField] private GameObject characterSelectScreen;

    private GameObject m_Player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        clientID = (int)OwnerClientId;
    }

    public void SetSelectedCharacter(int characterID)
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Spawn Point");
        GameObject player = null;
        switch (characterID)
        {
            case 0:
                player = Instantiate(solPrefab, spawnPoint.transform.position, Quaternion.identity);
                break;
            case 1:
                player = Instantiate(lunaPrefab, spawnPoint.transform.position, Quaternion.identity);
                break;
        }

        if (IsClient && m_Player != null)
            Destroy(m_Player);

        if (player != null)
        {
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);
            characterSelectScreen.SetActive(false);
            m_Player = player;
        }
    }
}
