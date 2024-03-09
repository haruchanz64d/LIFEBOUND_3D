using Unity.Netcode;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
public class LBNetworkManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI joinCodeUI;
    [SerializeField] private GameObject waitingCanvasAsHost;
    [SerializeField] private GameObject waitingCanvasAsClient;
    [SerializeField] private GameObject joinCodeErrorCanvas;
    [SerializeField] private GameObject loadingScreenCanvas;
    [SerializeField] private Image loadingBarFill;
    private int maxPlayers = 2;

    private void Awake()
    {
        joinCodeErrorCanvas.SetActive(false);
        loadingScreenCanvas.SetActive(false);
    }
    private async void Start()
    {
        waitingCanvasAsHost.SetActive(false);
        waitingCanvasAsClient.SetActive(false);

        InitializationOptions hostOptions = new InitializationOptions().SetProfile("Host");
        InitializationOptions clientOptions = new InitializationOptions().SetProfile("Client");

        await UnityServices.InitializeAsync(hostOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
        };

        if (AuthenticationService.Instance.IsAuthorized)
        {
            AuthenticationService.Instance.SignOut();
            await UnityServices.InitializeAsync(clientOptions);
        }
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateGame()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Here's the join code in case you cannot read the game's font (skill issue fr fr): {joinCode}");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            joinCodeUI.SetText(joinCode);

            NetworkManager.Singleton.StartHost();

            StartCoroutine(WaitingScreen());
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void JoinGame()
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: inputField.text);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
            StartCoroutine(LoadSceneAsync());
        }
        catch (RelayServiceException ex)
        {
            StartCoroutine(ShowError());
            Debug.Log(ex);
        }
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
            StartCoroutine(LoadSceneAsync());
        }
    }
    private IEnumerator LoadSceneAsync()
    {
        loadingScreenCanvas.SetActive(true);
        AsyncOperation async = SceneManager.LoadSceneAsync("Character Selection");
        while (!async.isDone)
        {
            float loadingValue = Mathf.Clamp01(async.progress / 0.9f);
            loadingBarFill.fillAmount = loadingValue;
            yield return null;
        }
    }

    private IEnumerator ShowError()
    {
        joinCodeErrorCanvas.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        joinCodeErrorCanvas.SetActive(false);
    }
}