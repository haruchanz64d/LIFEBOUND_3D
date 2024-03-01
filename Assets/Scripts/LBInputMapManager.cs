using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace LB.UI
{
    public class LBInputMapManager : MonoBehaviour
    {
        /*[SerializeField] private PlayerInput playerInput;
        [SerializeField] private Canvas pcCanvas, ps4Canvas, xboxCanvas;
        private void Update()
        {
            string currentControlScheme = playerInput.currentControlScheme;

            if (currentControlScheme == "Keyboard")
            {
                pcCanvas.enabled = true;
                ps4Canvas.enabled = false;
                xboxCanvas.enabled = false;
            }
            else if (currentControlScheme == "Gamepad")
            {
                var currentDevice = playerInput.devices[0];
                if (currentDevice.GetType().ToString() == "UnityEngine.InputSystem.DualShock.DualShock4GamepadHID")
                {
                    pcCanvas.enabled = false;
                    ps4Canvas.enabled = true;
                    xboxCanvas.enabled = false;
                }
                else
                {
                    pcCanvas.enabled = false;
                    ps4Canvas.enabled = false;
                    xboxCanvas.enabled = true;
                }
            }
        }*/
    }
}