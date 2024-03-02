using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;

namespace LB.Character
{
    public class LBUltimateManagement : MonoBehaviour
    {
        [SerializeField] private GameObject solModel;
        [SerializeField] private GameObject lunaModel;
        [SerializeField] private InputActionReference ultimateInput;
        [SerializeField] private Image soulSwapIcon;
        [SerializeField] private LBRoleAssigner roleAssigner;
        private float soulSwapCooldown = 30f; // 30 seconds
        private bool isSoulSwapCooldown;
        private bool isSoulSwapping;

        private void Update()
        {
            if (ultimateInput.action.triggered)
            {
                isSoulSwapCooldown = true;
            }

            if (isSoulSwapCooldown)
            {
                soulSwapIcon.fillAmount += 1 / soulSwapCooldown * Time.deltaTime;

                if (soulSwapIcon.fillAmount >= 1)
                {
                    soulSwapIcon.fillAmount = 0;
                    isSoulSwapCooldown = false;
                }
            }
        }
    }
}