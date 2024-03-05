using Mirror;
using TMPro;
using UnityEngine;
public class LBNetworkManager : MonoBehaviour
{
    private NetworkManager manager;
    [SerializeField] private Canvas selectionCanvas;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject solGameplayPrefab; // HOST
    [SerializeField] private GameObject lunaGameplayPrefab; // CLIENT

    private void Awake(){
        manager = GetComponent<NetworkManager>();
    }
    
    // Server + Client
    public void PlayAsSol(){
        GameObject solPlayer = Instantiate(solGameplayPrefab);
        NetworkServer.Spawn(solPlayer);
        selectionCanvas.enabled = false;
    }

    // Client + IP (Port)
    public void PlayAsLuna(){
        GameObject lunaPlayer = Instantiate(lunaGameplayPrefab);   
        NetworkServer.Spawn(lunaPlayer);
        selectionCanvas.enabled = false;
    }
}
