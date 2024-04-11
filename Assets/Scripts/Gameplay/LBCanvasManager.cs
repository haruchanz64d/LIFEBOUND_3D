using UnityEngine;

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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isGameplayPaused = true;
        mainCanvas.enabled = false;
        pauseCanvas.enabled = true;
    }

    public void OnGameplayResume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isGameplayPaused = false;
        mainCanvas.enabled = true;
        pauseCanvas.enabled = false;
    }
}
