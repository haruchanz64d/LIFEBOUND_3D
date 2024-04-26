using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.Managers;
using Assets.Scripts.Core;
using LB.Environment.Objects;
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
        [SerializeField] private NetworkVariable<int> currentHealth = new NetworkVariable<int>(100);
        [SerializeField] private TextMeshProUGUI healthText;
        [Space]
        [Header("Death Properties")]
        [SerializeField] private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);
        [Space]
        [Header("Soul Swap Properties")]
        [SerializeField] private NetworkVariable<float> soulSwapCooldown = new NetworkVariable<float>(0f);
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

        private void LateUpdate()
        {
            HandleMovement();
        }

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
                other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Lava"))
            {

            }
            if (other.CompareTag("Aqua Totem"))
            {

            }
        }

        #region Health System
        public void Heal(int amount)
        {
            if (!IsOwner)
            {
                // Server-side validation
                int currentHealthValue = currentHealth.Value;
                if (currentHealthValue + amount > 100)
                {
                    currentHealth.Value = 100;
                }
                else
                {
                    currentHealth.Value += amount;
                }

                // Notify clients of the health change
                UpdateHealthServerRpc(currentHealth.Value);
            }
            else
            {
                // Client-side prediction
                int currentHealthValue = currentHealth.Value;
                if (currentHealthValue + amount > 100)
                {
                    currentHealth.Value = 100;
                }
                else
                {
                    currentHealth.Value += amount;
                }

                // Notify server
                UpdateHealthServerRpc(currentHealth.Value);
            }
        }

        public void TakeDamage(int damage)
        {
            if (!IsOwner)
            {
                // Server-side validation
                int currentHealthValue = currentHealth.Value;
                if (currentHealthValue - damage < 0)
                {
                    currentHealth.Value = 0;
                }
                else
                {
                    currentHealth.Value -= damage;
                }

                // Notify clients of the health change
                UpdateHealthServerRpc(currentHealth.Value);
            }
            else
            {
                // Client-side prediction
                int currentHealthValue = currentHealth.Value;
                if (currentHealthValue - damage < 0)
                {
                    currentHealth.Value = 0;
                }
                else
                {
                    currentHealth.Value -= damage;
                }

                // Notify server
                UpdateHealthServerRpc(currentHealth.Value);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateHealthServerRpc(int health)
        {
            currentHealth.Value = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Burning DOT
        [ServerRpc(RequireOwnership = false)]
        public void ApplyBurningServerRpc(int damage, float interval)
        {
            if (currentHealth.Value > 0)
            {
                StartCoroutine(ApplyBurningCoroutine(damage, interval));
            }
        }

        IEnumerator ApplyBurningCoroutine(int damage, float interval)
        {
            while (currentHealth.Value > 0)
            {
                TakeDamage(damage);
                yield return new WaitForSeconds(interval);
            }
        }
        #endregion

        #region Death System
        public void Die()
        {
            if (!IsOwner)
            {
                isDead.Value = true;

                SetPlayerDieServeRpc(isDead.Value);
            }
            else
            {
                isDead.Value = true;

                SetPlayerDieServeRpc(isDead.Value);
            }

            StartCoroutine(RespawnCoroutine());
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerDieServeRpc(bool isDead)
        {
            this.isDead.Value = isDead;
        }

        IEnumerator RespawnCoroutine()
        {
            animator.SetTrigger("IsDead");
            yield return new WaitForSeconds(5f);

            // Respawn
            if (IsOwner)
            {
                transform.position = GameManagerRPC.Instance.GetRespawnPosition();
                currentHealth.Value = 100;
            }
            else
            {
                transform.position = GameManagerRPC.Instance.GetRespawnPosition();
                SetPlayerHealth(100);
            }
            animator.SetTrigger("IsAlive");
            isDead.Value = false;

            SetPlayerDieServeRpc(isDead.Value);
        }

        public void SetPlayerHealth(int health)
        {
            currentHealth.Value = health;
        }
        #endregion

        #region Soul Swap
        public void SwapSouls()
        {
            if (!IsOwner)
            {
                if (soulSwapCooldown.Value <= 0f)
                {
                    SwapSoulsServerRpc();
                    soulSwapCooldown.Value = 30f;

                    SetSoulSwapCooldownServerRpc(soulSwapCooldown.Value);
                }
            }
            else
            {
                if (soulSwapCooldown.Value <= 0f)
                {
                    SwapSoulsServerRpc();
                    soulSwapCooldown.Value = 30f;

                    SetSoulSwapCooldownServerRpc(soulSwapCooldown.Value);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetSoulSwapCooldownServerRpc(float cooldown)
        {
            soulSwapCooldown.Value = cooldown;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SwapSoulsServerRpc()
        {
            role.SwapCharacterModelRpc();

            AudioSource.PlayClipAtPoint(soulSwapSound, transform.position);
        }

        private void Update()
        {
            if (soulSwapCooldown.Value > 0f)
            {
                soulSwapCooldown.Value -= Time.deltaTime;
            }
            if (soulSwapCooldown.Value < 0f)
            {
                soulSwapCooldown.Value = 0f;
                role.ResetCharacterModelRpc();
            }
        }
        #endregion

        #region Moving Platform
        public void SetMovingPlatform(LBMovingPlatform movingPlatform)
        {
            if (movingPlatform != null)
            {
                transform.SetParent(movingPlatform.transform);
            }
            else
            {
                transform.SetParent(null);
            }
        }
        #endregion
    }
}