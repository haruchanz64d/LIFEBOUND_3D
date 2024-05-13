using Assets.Scripts.Managers;
using LB.Character;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HealthSystem : NetworkBehaviour
    {
        [Header("Health System")]
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>(100);
        private int maxHealth = 100;
        private bool isPlayerDead = false;
        public bool IsPlayerDead => isPlayerDead;

        [Header("Regeneration System")]
        private float regenerationRate = 2f;
        private float healTimer;

        [Header("Damage System - Lava")]
        private int lavaDamage = 5;
        private float damageTimer = 2f;
        private float damageTickInterval = 2f;
        [Header("Components")]
        private Animator animator;
        private GameManager gameManagerRPC;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private CameraShake cameraShake;

        [Header("Audio")]
        [SerializeField] private AudioClip hurtSound;

        [Header("Respawn System")]
        [SerializeField] private GameObject respawnCanvas;
        [SerializeField] private TMP_Text respawnText;
        private float respawnTimer;
        private bool isRespawning;
        private float respawnTime = 5f;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner) return;
            respawnCanvas.SetActive(false);
            currentHealth.Value = maxHealth;
            healthText.SetText($"Health: {currentHealth.Value}");
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            gameManagerRPC = FindObjectOfType<GameManager>();
        }

        #region RPC
        public void CheckForDeath()
        {
            CheckForPlayerDeathClientRpc();
        }

        [ClientRpc]
        public void CheckForPlayerDeathClientRpc()
        {
            GameManager.Instance.CheckForDeathServerRpc();
        }
        #endregion

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
            currentHealth.Value += amount;
            currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

            UpdateHealth(currentHealth.Value);
        }

        public void UpdateHealth(int health)
        {
            currentHealth.Value = health;
            healthText.SetText($"Health: {health}");
            Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        }
        #endregion


        #region Damage System
        public void ApplyLavaDoT()
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageTickInterval)
            {
                TakeDamage(lavaDamage);
                damageTimer = 0f;
            }
        }

        public void TakeDamage(int damage)
        {
            if (!IsLocalPlayer) return;
            if (isPlayerDead) return;

            if (currentHealth.Value <= 0)
            {
                isPlayerDead = true;
                animator.SetTrigger("IsDead");
                StartRespawnTimer(OwnerClientId);
            }

            Debug.Log($"Player {gameObject.name} taking damage: {damage}");

            currentHealth.Value -= damage;
            cameraShake.ShakeCamera();
            AudioManager.Instance.PlaySound(hurtSound);

            currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

            UpdateHealth(currentHealth.Value);
        }
        #endregion

        #region Death System

        public void SetPlayerHealth(int health)
        {
            currentHealth.Value = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Respawn System

        public void StartRespawnTimer(ulong clientId)
        {
            NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            player.GetComponent<Movement>().enabled = false;
            player.GetComponent<HealthSystem>().enabled = false;
            player.GetComponent<SoulSwap>().enabled = false;
            UpdateRespawnTimer();
        }
        private IEnumerator UpdateRespawnTimer()
        {            
            while(respawnTimer > 0f)
            {
                respawnTimer -= Time.deltaTime;
                respawnText.SetText($"Respawning in: {respawnTimer.ToString("F0")}");
                yield return new WaitForSeconds(Time.deltaTime);
            }
            RespawnPlayer(OwnerClientId);
        }

        private void RespawnPlayer(ulong clientId)
        {
            NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            player.GetComponent<Movement>().enabled = true;
            player.GetComponent<HealthSystem>().enabled = true;
            player.GetComponent<SoulSwap>().enabled = true;
            player.GetComponent<HealthSystem>().SetPlayerHealth(maxHealth);
            
            // Check if there are checkpoint interacted
            if(gameManagerRPC.LastInteractedCheckpointPosition != null)
            {
                player.transform.position = gameManagerRPC.LastInteractedCheckpointPosition;
            }
            else
            {
                player.transform.position = gameManagerRPC.DefaultSpawn.transform.position;
            }
        }
        #endregion
    }
}