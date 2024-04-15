using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LBServerManager : NetworkBehaviour
{
    private LBServerManager instance;
    public LBServerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = this;
            }
            return instance;
        }
    }
    public void GetConnectedClients()
    {
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            
        }
    }

    public void GetConnectedClientsCount()
    {
        Debug.Log(NetworkManager.ConnectedClientsList.Count);
    }

    public void OnClientDisconnect(ulong clientId)
    {
        Debug.Log("Client with id " + clientId + " disconnected");
        NetworkManager.Shutdown();
        SceneManager.LoadScene("Main Menu");
    }

    public void OnServerDisconnect()
    {
        Debug.Log("Server disconnected");
        NetworkManager.Shutdown();
        SceneManager.LoadScene("Main Menu");
    }
}
