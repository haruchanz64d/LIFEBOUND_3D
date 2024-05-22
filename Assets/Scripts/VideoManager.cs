using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField] private string sceneName;
    private HostManager hostManager;
    private void Awake()
    {
        if (hostManager == null)
        {
            Debug.LogError("HostManager not assigned or found on the GameObject.");
        }
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("VideoPlayer component not assigned or found on the GameObject.");
        }

        hostManager = GameObject.FindObjectOfType<HostManager>();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(DisconnectAndReturnToMainMenu());
    }

    private IEnumerator DisconnectAndReturnToMainMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }
            NetworkManager.Singleton.ConnectionApprovalCallback -= hostManager.ApprovalCheck;
        }

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(sceneName);
    }
}
