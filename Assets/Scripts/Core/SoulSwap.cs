﻿using Assets.Scripts.Managers;
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

            PlaySoulSwapAnimationClientRpc();

            NotifyOtherPlayerServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotifyOtherPlayerServerRpc()
        {
            NotifyOtherPlayerClientRpc();
        }

        [ClientRpc]
        private void NotifyOtherPlayerClientRpc()
        {
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

            SwapPlayerModelClientRpc();

            while (elapsedTime < cooldownDuration)
            {
                UpdateCooldownUIClientRpc(elapsedTime / cooldownDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UpdateCooldownUIClientRpc(1.0f);
            isSoulSwapActivated = false;
            isSoulSwapInCooldown = false;

            // Reset models and animations after cooldown
            ResetSoulSwapImageFillAmount();
            ResetPlayerModelClientRpc();
            ResetSoulSwapAnimationClientRpc();
        }

        private void ResetSoulSwapImageFillAmount()
        {
            UpdateCooldownUIClientRpc(0f);
        }

        [ClientRpc]
        private void PlaySoulSwapAnimationClientRpc()
        {
            animator.SetBool("IsSoulSwapEnabled", true);
        }

        [ClientRpc]
        private void ResetSoulSwapAnimationClientRpc()
        {
            animator.SetBool("IsSoulSwapEnabled", false);
        }

        [ClientRpc]
        private void SwapPlayerModelClientRpc()
        {
            AudioManager.Instance.PlaySound(soulSwapSound);
            role.SwapCharacterModelClientRpc();
        }

        [ClientRpc]
        private void ResetPlayerModelClientRpc()
        {
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