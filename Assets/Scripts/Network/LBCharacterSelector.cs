using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using UnityEngine.SceneManagement;
using UnityEngine;
using Utp;

public class LBCharacterSelector : MonoBehaviour
{
    private RelayNetworkManager relay;
    [SerializeField] private GameObject solPrefab;
    [SerializeField] private GameObject lunaPrefab;

    private void Awake()
    {
        relay = FindObjectOfType<RelayNetworkManager>();
    }

    public void PlayAsSol() 
    {
        
    }

    public void PlayAsLuna()
    {

    }
}
