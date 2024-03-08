using UnityEngine.UI;
using UnityEngine;
using Cinemachine;

public class LBCanvasManager : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas pauseCanvas;

    private bool isGameplayPaused = false;
    public bool GetGameplayPaused { 
        get 
        { return isGameplayPaused; }
        set 
        { isGameplayPaused = value; }
    }

    private void Awake()
    {
        OnGameplayResume();
    }

    public void OnGameplayPause()
    {
        isGameplayPaused = true;
        mainCanvas.enabled = false;
        pauseCanvas.enabled = true;
    }

    public void OnGameplayResume()
    {
        isGameplayPaused = false;
        mainCanvas.enabled = true;
        pauseCanvas.enabled = false;
    }
}
