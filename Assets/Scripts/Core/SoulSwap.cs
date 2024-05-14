using Assets.Scripts.Managers;
using System.Collections;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
namespace Assets.Scripts.Core
{
    public class SoulSwap : NetworkBehaviour
    {
        /// <summary>
        /// 1. Check if the player activates the soul swap skill
        /// 2. If the player activates the soul swap skill, play the animation
        /// 3. Set a flag during the animation to swap the player model that the player soul swapped.
        /// 4. Reset the animation after the swap is complete.
        /// </summary>
        [Header("Input Action Reference")]
        [SerializeField] private InputActionReference soulSwapInput;
        [Header("Soul Swap Properties")]
        [SerializeField] private Image soulSwapImage;
        [SerializeField] private bool isSoulSwapActive = false;
        private bool isSoulSwapInCooldown = false;
        private bool isSoulSwapActivated;
        public bool IsSoulSwapActivated
        {
            get => isSoulSwapActivated;
            set => isSoulSwapActivated = value;
        }
        [Space]
        [Header("Audio")]
        [SerializeField] private AudioClip soulSwapSound;
        [Header("Components")]
        private Role role;
        private Animator animator;
        private GameManager gameManagerRPC;
        private void Awake()
        {
            role = GetComponent<Role>();
            animator = GetComponent<Animator>();
            gameManagerRPC = FindObjectOfType<GameManager>();
        }


        private void Update()
        {
            // Check if we activated the skill or one of the player already activated it.
            if (!IsOwner) return;
            if (soulSwapInput.action.triggered && !isSoulSwapActivated)
            {
                ActivateSkill();
                
            }
            else if(!soulSwapInput.action.triggered && isSoulSwapActivated)
            {
                ActivateSkill();     
            }
        }

        #region Soul Swap
        // Play the soul swap animation
        // Swap the player model
        // After 30 seconds, reset the player model
        public void ActivateSkill()
        {
            // If the player already activated the skill, disable it.
            if (isSoulSwapActivated)
            {
                isSoulSwapActivated = false;
                // If the player is in cooldown, disable the cooldown.
                if (isSoulSwapInCooldown)
                {
                    HandleSoulswapCooldownClientRpc();
                }
            }
            // If the player did not activate the skill yet, enable it.
            else
            {
                isSoulSwapActivated = true;
                // Start the soul swap cooldown.
                HandleSoulswapCooldownClientRpc();
                // Play the soul swap animation.
                PlaySoulSwapAnimationClientRpc();
                // Wait for the animation to complete, then swap the player model.
                StartCoroutine(SwapPlayerModelAfterDelay());
            }
        }

        private IEnumerator SwapPlayerModelAfterDelay()
        {
            // Wait for the animation to complete.
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            // Swap the player model.
            SwapPlayerModelClientRpc();
            // Wait for a while, then reset the player model.
            yield return new WaitForSeconds(30f);
            // Reset the player model.
            ResetPlayerModelClientRpc();
            // Reset the soul swap animation.
            ResetSoulSwapAnimationClientRpc();
        }

        [ClientRpc]
        public void HandleSoulswapCooldownClientRpc()
        {
            if (isSoulSwapActivated)
            {
                isSoulSwapInCooldown = true;

                if (isSoulSwapInCooldown)
                {
                    soulSwapImage.fillAmount += 1 / GameManager.Instance.SoulSwapCooldown * Time.deltaTime;

                    if (soulSwapImage.fillAmount >= 1)
                    {
                        isSoulSwapInCooldown = false;
                        soulSwapImage.fillAmount = 0;
                    }
                }
            }
            else
            {
                isSoulSwapInCooldown = false;
                soulSwapImage.fillAmount = 0;
            }
        }

        [ClientRpc]
        public void PlaySoulSwapAnimationClientRpc()
        {
            animator.SetBool("IsSoulSwapEnabled", true);
        }

        [ClientRpc]
        public void ResetSoulSwapAnimationClientRpc()
        {
            animator.SetBool("IsSoulSwapEnabled", false);
        }

        [ClientRpc]
        public void SwapPlayerModelClientRpc()
        {
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.SwapCharacterModelRpc();
        }

        [ClientRpc]
        public void ResetPlayerModelClientRpc()
        {
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.ResetCharacterModelRpc();
        }
        #endregion
    }
}