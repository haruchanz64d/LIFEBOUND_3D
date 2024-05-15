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
        private GameManager gameManager;
        private HealthSystem healthSystem;
        private void Awake()
        {
            role = GetComponent<Role>();
            animator = GetComponent<Animator>();
            healthSystem = GetComponent<HealthSystem>();
        }

        public override void OnNetworkSpawn()
        {
            gameManager = GameManager.Instance;
        }

        private void LateUpdate()
        {
            if (healthSystem.IsPlayerDead) return;
            if (soulSwapInput.action.triggered && !isSoulSwapActivated) ActivateSkill();
        }

        #region Soul Swap
        // Play the soul swap animation
        // Swap the player model
        // After 30 seconds, reset the player model
        public void ActivateSkill()
        {
            if (isSoulSwapActivated)
            {
                isSoulSwapActivated = false;
                if (isSoulSwapInCooldown)
                {
                    StartCoroutine(SwapPlayerModelAfterDelay());
                }
            }
            else
            {
                isSoulSwapActivated = true;
                PlaySoulSwapAnimationClientRpc();
                StartCoroutine(SwapPlayerModelAfterDelay());
            }
        }

        private IEnumerator SwapPlayerModelAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            SwapPlayerModelClientRpc();
            yield return new WaitForSeconds(20f);
            ResetPlayerModelClientRpc();
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