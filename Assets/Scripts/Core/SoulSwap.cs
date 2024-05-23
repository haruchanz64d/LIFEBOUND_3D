using Assets.Scripts.Managers;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Core
{
    public class SoulSwap : NetworkBehaviour
    {
        [Header("Input Action Reference")]
        [SerializeField] private InputActionReference soulSwapInput;

        [Header("Soul Swap Properties")]
        [SerializeField] private Image soulSwapImage;
        private bool isSoulSwapActivated;
        private bool isSoulSwapInCooldown = false;

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

            if (soulSwapInput.action.triggered && !isSoulSwapActivated && !isSoulSwapInCooldown)
            {
                ActivateSkill();
            }
        }

        private void ActivateSkill()
        {
            isSoulSwapActivated = true;
            isSoulSwapInCooldown = true;

            Debug.Log("Activating Soul Swap");
            PlaySoulSwapAnimationClientRpc();
            NotifyOtherPlayerServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotifyOtherPlayerServerRpc()
        {
            Debug.Log("Notifying Other Player");
            NotifyOtherPlayerClientRpc();
        }

        [ClientRpc]
        private void NotifyOtherPlayerClientRpc()
        {
            Debug.Log("Client Notified of Soul Swap");
            if (!isSoulSwapActivated)
            {
                PlaySoulSwapAnimationClientRpc();
            }

            StartCoroutine(HandleCooldown());
        }

        private IEnumerator HandleCooldown()
        {
            float cooldownDuration = gameManager.SoulSwapCooldown;
            float elapsedTime = 0f;

            // Delay before swapping models to allow the animation to play
            yield return new WaitForSeconds(2f);

            Debug.Log("Swapping Player Model");
            SwapPlayerModelClientRpc();

            while (elapsedTime < cooldownDuration)
            {
                UpdateCooldownUIClientRpc(elapsedTime / cooldownDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UpdateCooldownUIClientRpc(1.0f);
            ResetSoulSwapImageFillAmount();

            Debug.Log("Resetting Player Model");
            ResetPlayerModel();
            ResetSoulSwapAnimationClientRpc();
            isSoulSwapActivated = false;
            isSoulSwapInCooldown = false;
        }

        private void ResetSoulSwapImageFillAmount()
        {
            UpdateCooldownUIClientRpc(0f);
        }

        [ClientRpc]
        private void PlaySoulSwapAnimationClientRpc()
        {
            Debug.Log("Playing Soul Swap Animation");
            animator.SetBool("IsSoulSwapEnabled", true);
        }

        [ClientRpc]
        private void ResetSoulSwapAnimationClientRpc()
        {
            Debug.Log("Resetting Soul Swap Animation");
            animator.SetBool("IsSoulSwapEnabled", false);
        }

        [ClientRpc]
        private void SwapPlayerModelClientRpc()
        {
            Debug.Log("Swapping Character Model");
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.SwapCharacterModelClientRpc();
        }

        private void ResetPlayerModel()
        {
            Debug.Log("Client Resetting Player Model");
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.ResetCharacterModelClientRpc();
        }

        [ClientRpc]
        private void UpdateCooldownUIClientRpc(float fillAmount)
        {
            soulSwapImage.fillAmount = fillAmount;
        }
    }
}
