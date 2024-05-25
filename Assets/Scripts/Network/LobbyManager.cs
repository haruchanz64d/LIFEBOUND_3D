using UnityEngine.UI;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System;
using System.Collections;
public class LobbyManager : MonoBehaviour
{
    [Header("Pre-Lobby")]
    [SerializeField] private GameObject preJoinLobbyCanvas;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject errorMessagePanel;
    [SerializeField] private TMP_Text errorMessageText;
    private void Awake()
    {
        preJoinLobbyCanvas.SetActive(true);
        errorMessagePanel.SetActive(false);
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
            if (AuthenticationService.Instance.IsSignedIn) return;
            errorMessagePanel.SetActive(true);
            errorMessageText.SetText($"Failed to authenticate player! Please check your Internet connection.");
            StartCoroutine(HideErrorMessage());
        }
    }

    public void CreateGame()
    {
        HostManager.Instance.StartHost();
    }

    public void JoinGame()
    {
        if(inputField.text.Length == 0)
        {
            errorMessagePanel.SetActive(true);
            errorMessageText.SetText("Please enter a valid join code.");
            StartCoroutine(HideErrorMessage());
            return;
        }
        else
        {
            ClientManager.Instance.StartClient(inputField.text);
        }
    }

    private IEnumerator HideErrorMessage()
    {
        yield return new WaitForSeconds(3f);
        errorMessagePanel.SetActive(false);
    }
}
