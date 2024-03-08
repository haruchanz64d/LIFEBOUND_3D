using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;

namespace LB.Character
{
    public class LBUltimateManagement : MonoBehaviour
    {
        [SerializeField] private InputActionReference ultimateInput;
        [SerializeField] private Image soulSwapIcon;
        private float soulSwapCooldown = 30f; // 30 seconds
        private bool isSoulSwapCooldown;
        private bool isSoulSwapping;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
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
                soulSwapIcon.fillAmount += 1 / soulSwapCooldown * Time.deltaTime;

                if (soulSwapIcon.fillAmount >= 1)
                {
                    soulSwapIcon.fillAmount = 0;
                    isSoulSwapCooldown = false;
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