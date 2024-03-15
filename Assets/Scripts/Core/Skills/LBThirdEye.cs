using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
namespace LB.Character
{
    public class LBThirdEye : MonoBehaviour
    {
        [SerializeField] private InputActionReference skillInput;
        [SerializeField] private Image thirdEyeSkillIcon;
        private float thirdEyeCooldown = 15f; // 15 seconds
        private bool isThirdEyeCooldown;
        [SerializeField] private TextMeshProUGUI thirdEyeCooldownText;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            thirdEyeCooldownText.enabled = false;
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
                thirdEyeCooldownText.enabled = true;
                thirdEyeSkillIcon.fillAmount += 1 / thirdEyeCooldown * Time.deltaTime;
                thirdEyeCooldownText.text = ((int)(thirdEyeSkillIcon.fillAmount * thirdEyeCooldown)).ToString();
                if (thirdEyeSkillIcon.fillAmount >= 1)
                {
                    thirdEyeSkillIcon.fillAmount = 0;
                    isThirdEyeCooldown = false;
                    thirdEyeCooldownText.enabled = false;
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