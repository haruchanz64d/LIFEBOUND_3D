/// <summary>
/// Manages the network functionality for the relay server.
/// </summary>

using Unity.Netcode;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    //[Header("Character Selection")]
    //[SerializeField] private GameObject characterSelection;
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenCanvas;
    [SerializeField] private GameObject loadingProgressBar;
    [SerializeField] private Image loadingProgressBarFill;
    [SerializeField] private TMP_Text loadingScreenTips;
    private int maxPlayers = 2;

    /// <summary>
    /// Initializes the pre-lobby, loading screen, and character selection.
    /// </summary>
    private void Awake()
    {
        preJoinLobbyCanvas.SetActive(true);
        loadingScreenCanvas.SetActive(false);
    }

    /// <summary>
    /// Initializes the authentication service and signs in anonymously.
    /// </summary>
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

    /// <summary>
    /// Creates a game session by allocating a server and setting up the relay server data.
    /// </summary>
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

    /// <summary>
    /// Joins an existing game session by connecting to the allocated server.
    /// </summary>
    /// <param name="inputField">The input field containing the join code.</param>
    public async void JoinGame(TMP_InputField inputField)
    {
        try
        {
            JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(inputField.text);

            RelayServerData serverData = new RelayServerData(join, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            NetworkManager.Singleton.StartClient();
            preJoinLobbyCanvas.SetActive(false);
            StartCoroutine(LoadingScreen());
        }
        catch (RelayServiceException ex) { Debug.Log(ex); }
    }

    /// <summary>
    /// Displays the waiting screen until all players have connected.
    /// </summary>
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
            StartCoroutine(LoadingScreen());
        }
    }

    private IEnumerator LoadingScreen()
    {
        yield return null;
        AsyncOperation async = SceneManager.LoadSceneAsync("Character Select");
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            loadingProgressBarFill.fillAmount += async.progress * 100;

            if (async.progress >= 0.9f)
            {
                loadingProgressBar.SetActive(false);

                loadingScreenTips.SetText("Press any key to continue...");
                if (Input.anyKeyDown)
                {
                    async.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }

    //THIS FUNCTIONS BELOW HERE ARE DEVELOPMENT PURPOSES ONLY
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Character Select", LoadSceneMode.Additive);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.LoadScene("Character Select", LoadSceneMode.Additive);
    }
}
