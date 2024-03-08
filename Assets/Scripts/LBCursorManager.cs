using UnityEngine;

public class LBCursorManager : MonoBehaviour
{
    private KeyCode lockKey = KeyCode.LeftAlt;
    private LBCanvasManager canvas;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        canvas = FindObjectOfType<LBCanvasManager>();
    }

    private void Update()
    {
        if (Input.GetKey(lockKey) || canvas.GetGameplayPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
    }
}
