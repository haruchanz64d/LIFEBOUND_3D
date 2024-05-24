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
            SwapPlayerModelServerRpc();

            while (elapsedTime < cooldownDuration)
            {
                UpdateCooldownUIClientRpc(elapsedTime / cooldownDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UpdateCooldownUIClientRpc(1.0f);
            ResetSoulSwapImageFillAmount();

            // Delay before resetting the player model to ensure animation completion
            yield return new WaitForSeconds(0.5f);

            // Reset the player model and soul swap animation
            ResetPlayerModelServerRpc();
            ResetSoulSwapAnimationClientRpc();

            // Reset activation flags
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
            Debug.Log("Playing Soul Swap Animation - Client");
            animator.SetBool("IsSoulSwapEnabled", true);
        }

        [ClientRpc]
        private void ResetSoulSwapAnimationClientRpc()
        {
            Debug.Log("Resetting Soul Swap Animation - Client");
            animator.SetBool("IsSoulSwapEnabled", false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SwapPlayerModelServerRpc()
        {
            Debug.Log("Server Swapping Player Model");
            SwapPlayerModelClientRpc();
        }

        [ClientRpc]
        private void SwapPlayerModelClientRpc()
        {
            Debug.Log("Client Swapping Character Model");
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.SwapCharacterModel();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ResetPlayerModelServerRpc()
        {
            Debug.Log("Server Resetting Player Model");
            ResetPlayerModelClientRpc();
        }

        [ClientRpc]
        private void ResetPlayerModelClientRpc()
        {
            Debug.Log("Client Resetting Player Model");
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.ResetCharacterModel();
        }

        [ClientRpc]
        private void UpdateCooldownUIClientRpc(float fillAmount)
        {
            soulSwapImage.fillAmount = fillAmount;
        }
    }
}
