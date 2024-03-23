using Unity.Netcode;
using UnityEngine;

public class LBLocalNetworkManager : NetworkBehaviour
{
    public void StartHost()
    {
        NetworkManager.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.StartClient();
    }
}
