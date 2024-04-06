using UnityEngine;
using Cinemachine;
public class LBCursorManager : MonoBehaviour
{
    private KeyCode lockKey = KeyCode.LeftAlt;
    [SerializeField] private CinemachineFreeLook freeLook;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKey(lockKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            freeLook.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            freeLook.enabled = true;
        }
    }
}
