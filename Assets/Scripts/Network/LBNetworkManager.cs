using Mirror;
using TMPro;
using UnityEngine;
public class LBNetworkManager : MonoBehaviour
{
    private NetworkManager manager;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject solGameplayPrefab; // HOST
    [SerializeField] private GameObject lunaGameplayPrefab; // CLIENT
    private void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    // Server + Client
    public void PlayAsSol(){
        manager.StartHost();
    }

    // Client + IP (Port)
    public void PlayAsLuna(){
        
    }
}
