using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using UnityEngine.SceneManagement;
using UnityEngine;
using Utp;
public class LBNetworkManager : MonoBehaviour
{
    private RelayNetworkManager relay;
    private int maxPlayers = 2;
    private string region;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public void CreateGame()
    {
        relay.StartRelayHost(maxPlayers, region);
    }

    public void JoinGame()
    {
        relay.JoinRelayServer();
    }
}
