using Assets.Scripts.Managers;
using LB.Character;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Core
{
    public class HealthSystem : NetworkBehaviour
    {
        [Header("Health System")]
        private int currentHealth;
        private int maxHealth = 100;
        private bool isPlayerDead = false;
        public bool IsPlayerDead => isPlayerDead;
        [Header("Regeneration System")]
        private float regenerationRate = 0.1f;
        private float healTimer;
        [Header("Damage System - Lava")]
        [SerializeField] private ParticleSystem burningParticle;
        private int lavaDamage = 3;
        private float damageTimer = 2f;
        private float damageTickInterval = 2f;
        [Header("Components")]
        private Animator animator;
        private GameManager gameManager;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private CameraShake cameraShake;

        [Header("Audio")]
        [SerializeField] private AudioClip hurtSound;

        [Header("Death System")]
        [SerializeField] private GameObject deathCanvas;
        [SerializeField] private TMP_Text deathText;
        private float disconnectTimer;
        private int timeBeforeDisconnect = 8;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameManager = GameManager.Instance;
            if (!IsOwner) return;
            deathCanvas.SetActive(false);
            currentHealth = maxHealth;
            healthText.SetText($"Health: {currentHealth}");
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            burningParticle.Stop();
        }

        #region Health System
        public void ApplyHealOverTime()
        {
            healTimer += Time.deltaTime;
            if (healTimer >= regenerationRate)
            {
                Heal(1);
                healTimer = 0f;
            }
        }

        public void Heal(int amount)
        {
            if (!IsLocalPlayer) return;
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHealth(currentHealth);
        }

        public void UpdateHealth(int health)
        {
            currentHealth = health;
            healthText.SetText($"Health: {health}");
            Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        #endregion

        #region Damage System
        public void ApplyLavaDoT()
        {
            burningParticle.Play();
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageTickInterval)
            {
                TakeDamage(lavaDamage);
                damageTimer = 0f;
            }
        }

        public void StopLavaDoT()
        {
            burningParticle.Stop();
        }

        public void TakeDamage(int damage)
        {
            if (!IsLocalPlayer) return;
            if (isPlayerDead) return;

            if (currentHealth <= 0)
            {
                isPlayerDead = true;
                animator.SetTrigger("IsDead");
                KillPlayer();
            }

            Debug.Log($"Player {gameObject.name} taking damage: {damage}");

            currentHealth -= damage;
            cameraShake.ShakeCamera();
            AudioManager.Instance.PlaySound(hurtSound);

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHealth(currentHealth);
        }
        #endregion

        #region Death System
        public void SetPlayerHealth(int health)
        {
            currentHealth = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Kill System
        public void KillPlayer()
        {
            isPlayerDead = true;
            deathCanvas.SetActive(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            disconnectTimer = timeBeforeDisconnect;
            StartCoroutine(ShowGameOverMessages());
        }

        private IEnumerator ShowGameOverMessages()
        {
            while (disconnectTimer > 0)
            {
                yield return new WaitForSeconds(1f);
                disconnectTimer--;
            }
            gameManager.DisconnectAllPlayers(OwnerClientId);
        }
        #endregion

        public void ForceKillPlayer()
        {
            isPlayerDead = true;
            animator.SetTrigger("IsDead");
            KillPlayer();
        }

    }
}