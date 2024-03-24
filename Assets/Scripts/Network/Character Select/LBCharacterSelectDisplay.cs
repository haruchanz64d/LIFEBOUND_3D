using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LBCharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private LBCharacterDatabase characterDatabase;
    [SerializeField] private Transform characterHold;
    [SerializeField] private LBCharacterSelectButton selectButton;
    [SerializeField] private LBPlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterName;

    private NetworkList<LBCharacterSelectState> players;

    private void Awake()
    {
        players = new NetworkList<LBCharacterSelectState>();    
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            LBCharacter[] allCharacters = characterDatabase.GetLBCharacters();

            foreach (var character in allCharacters)
            {
                var selectButtonInstance = Instantiate(selectButton, characterHold);
                selectButtonInstance.SetCharacter(this, character);
            }

            players.OnListChanged += HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        players.Add(new LBCharacterSelectState(clientId));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
       for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    public void Select(LBCharacter character)
    {
        characterName.SetText(character.DisplayName);
        characterInfoPanel.SetActive(true);
        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                players[i] = new LBCharacterSelectState(
                    players[i].ClientId,
                    characterId
                    );
            }
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<LBCharacterSelectState> changeEvent)
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableDisplay();
            }
        }
    }
}
