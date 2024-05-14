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
    [SerializeField] private TMP_InputField inputField;

    private void Awake()
    {
        preJoinLobbyCanvas.SetActive(true);
    }

    private async void Start()
    {
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
