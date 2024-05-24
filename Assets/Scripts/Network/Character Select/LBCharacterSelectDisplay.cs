using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LBCharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private LBCharacterDatabase characterDatabase;
    [SerializeField] private Transform characterHold;
    [SerializeField] private LBCharacterSelectButton selectButton;
    [SerializeField] private LBPlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private Transform introSpawpoint;
    [SerializeField] private Button lockinButton;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text errorText;

    private GameObject introInstance;

    private List<LBCharacterSelectButton> characterButtons = new List<LBCharacterSelectButton>();

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
                characterButtons.Add(selectButtonInstance);
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
        if (IsHost)
        {
            joinCodeText.SetText(HostManager.Instance.joinCode);
            errorText.SetText(HostManager.Instance.warningText);
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
            if (players[i].ClientId != clientId) { continue; }

            players.RemoveAt(i);
            break;
        }
    }

    public void Select(LBCharacter character)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (players[i].IsLockedIn) { return; }

            if (players[i].CharacterId == character.Id) { return; }

            if (IsCharacterTaken(character.Id, false)) { return; }
        }

        characterName.SetText(character.DisplayName);
        characterInfoPanel.SetActive(true);

        if (introInstance != null)
        {
            Destroy(introInstance);
        }

        introInstance = Instantiate(character.IntroCharacter, introSpawpoint);

        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }
            if (!characterDatabase.IsValidCharacterId(characterId)) { return; }
            if (IsCharacterTaken(characterId, true)) { return; }
            players[i] = new LBCharacterSelectState(
                players[i].ClientId,
                characterId,
                players[i].IsLockedIn
                );
        }
    }

    public void LockedIn()
    {
        LockInServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }
            if (!characterDatabase.IsValidCharacterId(players[i].CharacterId)) { return; }
            if (IsCharacterTaken(players[i].CharacterId, true)) { return; }
            players[i] = new LBCharacterSelectState(
                players[i].ClientId,
                players[i].CharacterId,
                true
                );
        }

        foreach (var player in players)
        {
            if (!player.IsLockedIn) { return; }
        }

        foreach (var player in players)
        {
            HostManager.Instance.SetCharacter(player.ClientId, player.CharacterId);
        }

        HostManager.Instance.StartGame();
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

        foreach (var button in characterButtons)
        {
            if (button.IsDisabled) { continue; }
            if (IsCharacterTaken(button.Character.Id, false))
            {
                button.SetDisabled();
            }
        }

        foreach (var player in players)
        {
            if (player.ClientId != NetworkManager.Singleton.LocalClientId) continue;
            if (player.IsLockedIn)
            {
                lockinButton.interactable = false;
                break;
            }

            if (IsCharacterTaken(player.CharacterId, false))
            {
                lockinButton.interactable = false;
                break;
            }
            lockinButton.interactable = true;
            break;
        }
    }

    private bool IsCharacterTaken(int characterId, bool checkAll)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!checkAll)
            {
                if (players[i].ClientId == NetworkManager.Singleton.LocalClientId) { continue; }
            }
            if (players[i].IsLockedIn && players[i].CharacterId == characterId) { return true; }
        }
        return false;
    }
}
