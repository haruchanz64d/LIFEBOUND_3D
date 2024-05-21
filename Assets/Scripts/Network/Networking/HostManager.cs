using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
public class HostManager : NetworkBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string characterSelectName = "Character Select";
    [SerializeField] private string gameplaySceneName = "Gameplay Scene";
    [SerializeField] private string developmentSceneName = "Developer Room";
    [SerializeField] private int maxConnectionCount = 2;
    public static HostManager Instance { get; private set; }
    private bool hasGameStarted;
    public string joinCode { get; private set; }
    public Dictionary<ulong, ClientData> ClientData { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public async void StartHost()
    {
        Allocation allocation;
        try
        {
             allocation = await RelayService.Instance.CreateAllocationAsync(maxConnectionCount);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay service allocation request failed! {e.Message}");
            throw;
        }

        Debug.Log($"Client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"Host: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"Server: {allocation.AllocationId}");


        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay get join code request failed! {e.Message}");
            throw;
        }

        var relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

        ClientData = new Dictionary<ulong, ClientData>();

        NetworkManager.Singleton.StartHost();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (ClientData.Count >= maxConnectionCount || hasGameStarted) { response.Approved = false; return; } 
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);

        Debug.Log($"Added client {request.ClientNetworkId}");
    }

    private void OnNetworkReady()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.LoadScene(characterSelectName, LoadSceneMode.Single); 
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientData.ContainsKey(clientId))
        {
            if (ClientData.Remove(clientId))
            {
                Debug.Log($"Removed client {clientId}");
            }
        }
    }

    public void SetCharacter(ulong clientId, int characterId)
    {
        if (ClientData.TryGetValue(clientId, out ClientData data))
        {
            data.characterId = characterId;
        }
    }

    public void StartGame()
    {
        hasGameStarted = true;
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}
