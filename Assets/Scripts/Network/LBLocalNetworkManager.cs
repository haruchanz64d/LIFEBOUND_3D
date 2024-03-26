using Unity.Netcode;
using UnityEngine;

public class LBLocalNetworkManager : NetworkBehaviour
{
    public GameObject canvas;
    public void StartHost()
    {
        NetworkManager.StartHost();
        DestroyCanvas();
    }

    public void StartClient()
    {
        NetworkManager.StartClient();
        DestroyCanvas();
    }

    public void DestroyCanvas()
    {
        Destroy(canvas);
    }
}
