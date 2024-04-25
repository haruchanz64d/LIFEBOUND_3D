using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.Managers;
using Assets.Scripts.Core;
namespace LB.Character
{
    /// <summary>
    /// Represents the player character in the game.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        [Header("Input Action References")]
        [SerializeField] private InputActionReference movementInput;
        [SerializeField] private InputActionReference jumpInput;
        [SerializeField] private InputActionReference soulSwapInput;
        [Space]
        [Header("Movement Properties")]
        private float movementSpeed = 12f;
        private float jumpHeight = 8f;
        private float jumpButtonGracePeriod;
        private float ySpeed;
        private float originalStepOffset;
        private float? lastGroundedTime;
        private float? jumpButtonPressedTime;
        private bool isGrounded;
        private bool isJumping;
        private float jumpHorizontalSpeed = 1f;
        [Space]
        [Header("Health Properties")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField] private TMP_Text healthText;
        private bool isDead;
        public bool IsDead { get; set; }
        [Space]
        [Header("Components")]
        private CharacterController controller;
        [SerializeField] LBRole role;
        [SerializeField] private Animator animator;
        [Space]
        [Header("Camera")]
        [SerializeField] private new CinemachineFreeLook camera;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private AudioListener listener;
        [SerializeField] private Transform cameraTransform;
        [Space]
        [Header("Audio")]
        [SerializeField] private AudioClip soulSwapSound;
        [SerializeField] private AudioClip hurtSound;
        [Space]
        [Header("Soul Swap Skill")]
        private float soulSwapCooldown = 30f;
        private float soulSwapTimer;
        private float soulSwapDuration = 10f;
        [SerializeField] private Image soulSwapCooldownImage;
        private bool isSoulSwapping;
        public bool IsSoulSwapping { get => isSoulSwapping; set => isSoulSwapping = value; }

        /// <summary>
        /// Called when the player is spawned in the network.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                listener.enabled = true;
                camera.Priority = 1;
                minimapCamera.depth = 1;
            }
            else
            {
                camera.Priority = 0;
                minimapCamera.depth = 0;
            }
            currentHealth = maxHealth;
        }
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            originalStepOffset = controller.stepOffset;
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            isDead = false;
        }

        private void Update()
        {
            if (IsDead) return;
            if (isSoulSwapping)
            {
                HandleSoulSwap();
            }
            UpdateHealthUI();
            CheckForHealth();
        }

        private void LateUpdate()
        {
            if (IsDead) return;
            HandleMovement();
        }

        #region Health System
        private void UpdateHealthUI()
        {
            if (!IsLocalPlayer) return;
            healthText.SetText($"HP: {currentHealth}");
        }

        private void CheckForHealth()
        {
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
                GameManagerRPC.Instance.RespawnBothPlayersRpc(OwnerClientId);
                StartCoroutine(AnimateBeforeRespawn());
            }
        }

        #endregion

        #region Soul Swap
        /// <summary>
        /// Handles the player's soul swapping.
        /// </summary>
        private void HandleSoulSwap()
        {
            if (!IsLocalPlayer) return;
            if (isSoulSwapping) return;
            if (soulSwapInput.action.WasPerformedThisFrame() && !isSoulSwapping && soulSwapTimer <= 0)
            {
                LBAudioManager.Instance.PlaySound(soulSwapSound);
                StartCoroutine(HandleSoulSwapCoroutine());
                GameManagerRPC.Instance.StartSoulSwapCooldownRpc();
            }
        }


        private IEnumerator HandleSoulSwapCoroutine()
        {
            animator.SetBool("IsSoulSwapEnabled", true);
            isSoulSwapping = true;
            yield return new WaitForSeconds(1f);
            animator.SetBool("IsSoulSwapEnabled", false);
            yield return new WaitForSeconds(1f);

            role.SwapCharacterModelRpc();

            yield return new WaitForSeconds(soulSwapDuration);
            StartCoroutine(SoulSwapCooldown());
            role.ResetCharacterModelRpc();
            isSoulSwapping = false;
        }

        public void SoulSwapTimer()
        {
            isSoulSwapping = true;
        }
        private IEnumerator SoulSwapCooldown()
        {
            soulSwapCooldownImage.fillAmount = 1;
            soulSwapTimer = soulSwapCooldown;
            while (soulSwapTimer > 0)
            {
                soulSwapTimer -= Time.deltaTime;
                soulSwapCooldownImage.fillAmount = soulSwapTimer / soulSwapCooldown;
                yield return null;
            }
            isSoulSwapping = false;
        }

        #endregion
        #region Movement
        /// <summary>
        /// Handles the player's movement.
        /// </summary>
        private void HandleMovement()
        {
            if (!IsLocalPlayer) return;
            Vector2 movement = movementInput.action.ReadValue<Vector2>();
            Vector3 direction = new Vector3(movement.x, 0f, movement.y);
            float magnitude = Mathf.Clamp01(direction.magnitude) * movementSpeed;
            direction.Normalize();

            animator.SetFloat("Movement", magnitude, 0.05f, Time.deltaTime);

            direction = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * direction;

            ySpeed += Physics.gravity.y * Time.deltaTime;

            if (controller.isGrounded) lastGroundedTime = Time.time;

            if (jumpInput.action.WasPerformedThisFrame()) jumpButtonPressedTime = Time.time;

            if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
            {
                controller.stepOffset = originalStepOffset;
                ySpeed = -0.5f;
                animator.SetBool("IsGrounded", true);
                isGrounded = true;
                animator.SetBool("IsJumping", false);
                isJumping = false;
                animator.SetBool("IsFalling", false);

                if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
                {
                    ySpeed = jumpHeight;
                    animator.SetBool("IsJumping", true);
                    isJumping = true;
                    jumpButtonPressedTime = null;
                    lastGroundedTime = null;
                }
            }
            else
            {
                controller.stepOffset = 0f;
                animator.SetBool("IsGrounded", false);
                isGrounded = false;

                if ((isJumping && ySpeed < 0) || ySpeed < -2) animator.SetBool("IsFalling", true);
            }
            Vector3 velocity = direction * magnitude;
            velocity.y = ySpeed;

            controller.Move(velocity * Time.deltaTime);

            if (isGrounded == false)
            {
                Vector3 _velocity = direction * magnitude * jumpHorizontalSpeed;
                _velocity.y = ySpeed;
                controller.Move(_velocity * Time.deltaTime);
            }

            if (direction != Vector3.zero)
            {
                animator.SetBool("IsMoving", true);
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }
        }

        /// <summary>
        /// OnAnimatorMove is called after the animator has updated the character's position and rotation.
        /// </summary>
        private void OnAnimatorMove()
        {
            if (isGrounded)
            {
                Vector3 velocity = animator.deltaPosition;
                velocity.y = ySpeed * Time.deltaTime;

                controller.Move(velocity);
            }
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                GameManagerRPC.Instance.SetCheckpoint = other.transform;
                other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsLocalPlayer) return; // Only apply damage if this is the local player

            if (other.CompareTag("Lava"))
            {
                GameManagerRPC.Instance.ShareLavaDamageRpc(OwnerClientId);
            }
            if (other.CompareTag("Aqua Totem"))
            {
                RegainHealth(1f);
            }
        }


        public void RegainHealth(float health)
        {
            currentHealth += (int)health;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            UpdateHealthUI();
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= (int)damage;
            UpdateHealthUI();
        }

        public IEnumerator AnimateBeforeRespawn()
        {
            animator.SetTrigger("IsDead");
            yield return new WaitForSeconds(1f);
            GameManagerRPC.Instance.RespawnPlayerServerRpc(OwnerClientId);
            isDead = false;
            currentHealth = maxHealth;
            animator.SetTrigger("IsAlive");
        }

    }
}