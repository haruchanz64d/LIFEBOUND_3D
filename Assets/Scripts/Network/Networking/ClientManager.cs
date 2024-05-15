using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public async void StartClient(string joinCode)
    {
        JoinAllocation joinAllocation;

        try
        {
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError($"Relay get join code request failed!");
            throw;
        }

        Debug.Log($"Client: {joinAllocation.ConnectionData[0]} {joinAllocation.ConnectionData[1]}");
        Debug.Log($"Host: {joinAllocation.ConnectionData[0]} {joinAllocation.ConnectionData[1]}");
        Debug.Log($"Client: {joinAllocation.AllocationId}");

        var relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
    }
}
