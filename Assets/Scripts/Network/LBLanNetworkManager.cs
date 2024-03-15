using Unity.Netcode;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;

public class LBLanNetworkManager : MonoBehaviour
{
    [Header("Pre-Lobby")]
    [SerializeField] private GameObject preJoinLobbyCanvas;
    [Header("Queue (Host)")]
    [SerializeField] private GameObject waitingCanvasAsHost;
    [SerializeField] private TextMeshProUGUI ipText;
    [Header("Queue (Client)")]
    [SerializeField] private GameObject waitingCanvasAsClient;
    [SerializeField] private TMP_InputField inputField;
    [Header("Character Selection")]
    [SerializeField] private GameObject characterSelection;
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenCanvas;
    [SerializeField] private Image loadingBarFill;
    private int maxPlayers = 2;
    [Header("LAN Network")]
    [SerializeField] private string ipAddress;

    private void Awake()
    {
        preJoinLobbyCanvas.SetActive(true);
        loadingScreenCanvas.SetActive(false);
        characterSelection.SetActive(false);
    }
    private void Start()
    {
        waitingCanvasAsHost.SetActive(false);
        waitingCanvasAsClient.SetActive(false);
    }

    public void CreateGame()
    {
        NetworkManager.Singleton.StartHost();
        preJoinLobbyCanvas.SetActive(false);
        StartCoroutine(WaitingScreen());

        GetLocalIPAddress();
    }

    public void JoinGame(TMP_InputField inputField)
    {
        if (inputField.text == null) return;
        NetworkManager.Singleton.StartClient();
        preJoinLobbyCanvas.SetActive(false);
        characterSelection.SetActive(true);
    }

    private IEnumerator WaitingScreen()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            waitingCanvasAsHost.SetActive(true);
            while (NetworkManager.Singleton.ConnectedClients.Count < maxPlayers)
            {
                yield return null;
            }
            waitingCanvasAsHost.SetActive(false);
            characterSelection.SetActive(true);
        }
    }

    private void GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach(var ip in host.AddressList)
        {
            if(ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipText.SetText(ip.ToString());
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void SetIPAddress()
    { 
        UnityTransport transport = GetComponent<UnityTransport>();

        transport.ConnectionData.Address = ipAddress;
    }
}