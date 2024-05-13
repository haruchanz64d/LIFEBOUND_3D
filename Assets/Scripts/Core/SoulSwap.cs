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
        [Header("Input Action Reference")]
        [SerializeField] private InputActionReference soulSwapInput;
        [Header("Soul Swap Properties")]
        [SerializeField] private Image soulSwapImage;
        [SerializeField] private bool isSoulSwapActive = false;
        private bool isSoulSwapInCooldown = false;
        public float SoulSwapCooldown { get; set; }
        private bool isSoulSwapReady;
        public bool IsSoulSwapReady
        {
            get => isSoulSwapReady = !isSoulSwapInCooldown;

            set
            {
                isSoulSwapReady = value;
            }
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
            if(!IsOwner) return;
            if (soulSwapInput.action.triggered)
            {
                ActivateSkill();
            }
            if (isSoulSwapInCooldown)
            {
                soulSwapImage.fillAmount += 1 / gameManagerRPC.SoulSwapCooldown * Time.deltaTime;
            }
        }

        #region Soul Swap
        public async void ActivateSkill()
        {
            if (isSoulSwapInCooldown) return;
            PlaySoulSwapAnimationClientRpc();
            await Task.Delay(1000);
            SwapPlayerModelClientRpc();
            SoulSwapCooldownRoutine();
        }

        private IEnumerator SoulSwapCooldownRoutine()
        {
            yield return new WaitForSeconds(gameManagerRPC.SoulSwapCooldown);
            soulSwapImage.fillAmount = 0;
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