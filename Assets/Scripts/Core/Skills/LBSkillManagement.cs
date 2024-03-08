using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
namespace LB.Character
{
    public class LBSkillManagement : MonoBehaviour
    {
        [SerializeField] private InputActionReference skillInput;
        [SerializeField] private Image thirdEyeSkillIcon;
        private float thirdEyeCooldown = 15f; // 15 seconds
        private bool isThirdEyeCooldown;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (skillInput.action.WasPressedThisFrame())
            {
                if (isThirdEyeCooldown) return;
                isThirdEyeCooldown = true;
                StartCoroutine(OnSkillAnimationPlay());
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

        private IEnumerator OnSkillAnimationPlay()
        {
            animator.SetBool("IsThirdEyeEnabled", true);
            yield return new WaitForSeconds(2.5f);
            animator.SetBool("IsThirdEyeEnabled", false);
        }
    }
}