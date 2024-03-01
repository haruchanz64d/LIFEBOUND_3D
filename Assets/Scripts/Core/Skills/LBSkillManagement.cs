using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;
namespace LB.Character
{
    public class LBSkillManagement : NetworkBehaviour
    {
        [SerializeField] private InputActionReference skillInput;
        [SerializeField] private Image thirdEyeSkillIcon;
        private float thirdEyeCooldown = 15f; // 15 seconds
        private bool isThirdEyeCooldown;

        private void Update()
        {
            if (skillInput.action.triggered)
            {
                isThirdEyeCooldown = true;
            }

            if (isThirdEyeCooldown)
            {
                thirdEyeSkillIcon.fillAmount += 1 / thirdEyeCooldown * Time.deltaTime;

                if (thirdEyeSkillIcon.fillAmount >= 1)
                {
                    thirdEyeSkillIcon.fillAmount = 0;
                    isThirdEyeCooldown = false;
                }
            }
        }
    }
}