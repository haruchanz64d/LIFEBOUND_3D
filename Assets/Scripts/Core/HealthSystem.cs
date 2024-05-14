using Assets.Scripts.Managers;
using LB.Character;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

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
        private float regenerationRate = 2f;
        private float healTimer;
        [Header("Damage System - Lava")]
        [SerializeField] private ParticleSystem burningParticle;
        private int lavaDamage = 10;
        private float damageTimer = 2f;
        private float damageTickInterval = 2f;
        [Header("Components")]
        private Animator animator;
        private GameManager gameManager;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private CameraShake cameraShake;

        [Header("Audio")]
        [SerializeField] private AudioClip hurtSound;

        [Header("Respawn System")]
        [SerializeField] private GameObject respawnCanvas;
        [SerializeField] private TMP_Text respawnText;
        private float respawnTimer;
        private float respawnTime = 5f;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            if (!IsOwner) return;
            respawnCanvas.SetActive(false);
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
                Heal(2);
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
                StartRespawnTimer();
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

        #region Respawn System
        public void StartRespawnTimer()
        {
            respawnCanvas.SetActive(true);
            respawnTimer = respawnTime;
            StartCoroutine(UpdateRespawnTimer());
        }

        private IEnumerator UpdateRespawnTimer()
        {
            while (respawnTimer > 0f)
            {
                respawnTimer -= Time.deltaTime;
                respawnText.SetText($"Respawning in: {respawnTimer.ToString("F0")}");
                yield return new WaitForSeconds(Time.deltaTime);
            }
            RespawnPlayer();
        }

        private void RespawnPlayer()
        {
            respawnCanvas.SetActive(false);
            // Set player position to last checkpoint 
            // Note: Does not work in multiplayer.
            if (gameManager.LastInteractedCheckpointPosition != null)
            {
                transform.position = gameManager.LastInteractedCheckpointPosition;
            }
            else
            {
                transform.position = gameManager.DefaultSpawn.transform.position;
            }
            isPlayerDead = false;
            animator.SetTrigger("IsAlive");
            currentHealth = maxHealth;
        }
        #endregion
    }
}