using UnityEngine;
using Unity.Netcode;
using TMPro;
public class LBGameManager : NetworkBehaviour
{
    public static LBGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}