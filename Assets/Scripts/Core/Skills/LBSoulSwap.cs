using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;
using TMPro;

namespace LB.Character
{
    public class LBSoulSwap : MonoBehaviour
    {
        [SerializeField] private InputActionReference ultimateInput;
        [SerializeField] private Image soulSwapIcon;
        [SerializeField] private TextMeshProUGUI soulSwapCooldownText;
        private float soulSwapCooldown = 30f; // 30 seconds
        private bool isSoulSwapCooldown;
        private bool isSoulSwapping;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            soulSwapCooldownText.enabled = false;
        }

        private void Update()
        {
            if (ultimateInput.action.WasPressedThisFrame())
            {
                if (isSoulSwapCooldown) return;
                isSoulSwapCooldown = true;
                StartCoroutine(OnUltimateAnimationPlay());
            }

            if (isSoulSwapCooldown)
            {
                soulSwapCooldownText.enabled = true;
                soulSwapIcon.fillAmount += 1 / soulSwapCooldown * Time.deltaTime;
                soulSwapCooldownText.text = ((int)(soulSwapIcon.fillAmount * soulSwapCooldown)).ToString();
                if (soulSwapIcon.fillAmount >= 1)
                {
                    soulSwapIcon.fillAmount = 0;
                    isSoulSwapCooldown = false;
                    soulSwapCooldownText.enabled = false;
                }
            }
        }

        private IEnumerator OnUltimateAnimationPlay()
        {
            animator.SetBool("IsSoulSwapEnabled", true);
            yield return new WaitForSeconds(2.5f);
            animator.SetBool("IsSoulSwapEnabled", false);
        }
    }
}