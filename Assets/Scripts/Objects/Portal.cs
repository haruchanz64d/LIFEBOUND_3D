using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Portal : NetworkBehaviour
{
    private int maxPlayers = 2;
    private HashSet<NetworkObject> playersInTrigger = new HashSet<NetworkObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out NetworkObject networkObject))
        {
            if (networkObject.IsPlayerObject)
            {
                playersInTrigger.Add(networkObject);
                CheckAndLoadNextScene();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out NetworkObject networkObject))
        {
            if (networkObject.IsPlayerObject)
            {
                playersInTrigger.Remove(networkObject);
            }
        }
    }

    private void CheckAndLoadNextScene()
    {
        if (playersInTrigger.Count >= maxPlayers)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Ending Scene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
