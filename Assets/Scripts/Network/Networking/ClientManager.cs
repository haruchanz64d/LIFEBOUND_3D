using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class ClientManager : MonoBehaviour
{
    [Header("UI Error Handling")]
    [SerializeField] private GameObject errorMessagePanel;
    [SerializeField] private TMP_Text errorMessageText;
    public static ClientManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        errorMessagePanel.SetActive(false);
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
            errorMessagePanel.SetActive(true);
            errorMessageText.SetText("Join code is invalid, please double-check the code and try again.");
            StartCoroutine(HideErrorMessage());
            throw;
        }

        Debug.Log($"Client: {joinAllocation.ConnectionData[0]} {joinAllocation.ConnectionData[1]}");
        Debug.Log($"Host: {joinAllocation.ConnectionData[0]} {joinAllocation.ConnectionData[1]}");
        Debug.Log($"Client: {joinAllocation.AllocationId}");

        var relayServerData = new RelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
    }

    private IEnumerator HideErrorMessage()
    {
        yield return new WaitForSeconds(3f);
        errorMessagePanel.SetActive(false);
    }
}
