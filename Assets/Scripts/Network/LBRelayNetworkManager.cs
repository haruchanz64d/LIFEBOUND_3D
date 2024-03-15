using Unity.Netcode;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LBRelayNetworkManager : MonoBehaviour
{
    [Header("Pre-Lobby")]
    [SerializeField] private GameObject preJoinLobbyCanvas;
    [Header("Queue (Host)")]
    [SerializeField] private GameObject waitingCanvasAsHost;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [Header("Queue (Client)")]
    [SerializeField] private GameObject waitingCanvasAsClient;
    [SerializeField] private TMP_InputField inputField;
    [Header("Character Selection")]
    [SerializeField] private GameObject characterSelection;
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenCanvas;
    [SerializeField] private Image loadingBarFill;
    private int maxPlayers = 2;

    private void Awake()
    {
        preJoinLobbyCanvas.SetActive(true);
        loadingScreenCanvas.SetActive(false);
        characterSelection.SetActive(false);
    }
    private async void Start()
    {
        waitingCanvasAsHost.SetActive(false);
        waitingCanvasAsClient.SetActive(false);

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateGame()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            joinCodeText.SetText(joinCode);

            RelayServerData serverData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            NetworkManager.Singleton.StartHost();
            preJoinLobbyCanvas.SetActive(false);
            StartCoroutine(WaitingScreen());
        }

        catch (RelayServiceException ex) { Debug.Log(ex); }
    }

    public async void JoinGame(TMP_InputField inputField)
    {
        try
        {
            JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(inputField.text);

            RelayServerData serverData = new RelayServerData(join, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            NetworkManager.Singleton.StartClient();
            preJoinLobbyCanvas.SetActive(false);
            characterSelection.SetActive(true);
        }
        catch (RelayServiceException ex) { Debug.Log(ex); }
    }

    private IEnumerator WaitingScreen()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            waitingCanvasAsHost.SetActive(true);
            while (NetworkManager.Singleton.ConnectedClients.Count < maxPlayers)
            {
                yield return null;
            }
            waitingCanvasAsHost.SetActive(false);
            characterSelection.SetActive(true);
        }
    }
}
