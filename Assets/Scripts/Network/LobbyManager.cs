using UnityEngine.UI;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System;

public class LobbyManager : MonoBehaviour
{
    [Header("Pre-Lobby")]
    [SerializeField] private GameObject preJoinLobbyCanvas;
    [Header("Queue (Host)")]
    [SerializeField] private GameObject waitingCanvasAsHost;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [Header("Queue (Client)")]
    [SerializeField] private GameObject waitingCanvasAsClient;
    [SerializeField] private TMP_InputField inputField;
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenCanvas;
    [SerializeField] private GameObject loadingProgressBar;
    [SerializeField] private Image loadingProgressBarFill;
    [SerializeField] private TMP_Text loadingScreenTips;

    private void Awake()
    {
        preJoinLobbyCanvas.SetActive(true);
        loadingScreenCanvas.SetActive(false);
    }

    private async void Start()
    {
        waitingCanvasAsHost.SetActive(false);
        waitingCanvasAsClient.SetActive(false);

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }

        catch (Exception e)
        {
            Debug.LogError($"{e.Message}");
        }
    }

    public void CreateGame()
    {
        HostManager.Instance.StartHost();
    }

    public void JoinGame()
    {
        ClientManager.Instance.StartClient(inputField.text);
    }
}
