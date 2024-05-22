using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Portal : NetworkBehaviour
{
    private int maxPlayers = 2;
    private int currentPlayers;
    private GameManager gameManager;

    private void Update()
    {
        if (!IsServer) return;

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        CheckAndLoadNextScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayers++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            currentPlayers--;
        }
    }

    private void CheckAndLoadNextScene()
    {
        if (currentPlayers >= maxPlayers)
        {
            if(gameManager.IsCollectionGoalReached)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Ending Scene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }
}
