using Assets.Scripts.Managers;
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
        [SerializeField] public Image soulSwapImage;
        public bool isSoulSwapActivated;
        public bool isSoulSwapInCooldown = false;
        public AudioClip soulSwapAudioClip;
        [Header("Components")]
        private HealthSystem healthSystem;
        private GameManager gameManager;

        private void Awake()
        {
            healthSystem = GetComponent<HealthSystem>();
        }
        
        public override void OnNetworkSpawn()
        {
            gameManager = GameManager.Instance;
        }

        private void LateUpdate()
        {
            if (healthSystem.IsPlayerDead) return;

            if (soulSwapInput.action.triggered)
            {
                RequestActivateSkillServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestActivateSkillServerRpc()
        {
            if (!isSoulSwapActivated && !isSoulSwapInCooldown)
            {
                gameManager.ActivateSoulSwapClientRpc();
                gameManager.StartSoulSwapCooldown();
            }
        }

        public void ResetSoulSwapState()
        {
            isSoulSwapActivated = false;
            isSoulSwapInCooldown = false;
            soulSwapImage.fillAmount = 0f;
        }
    }
}
