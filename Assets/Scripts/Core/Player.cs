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
        [Space]// 10 seconds
        [SerializeField] private TextMeshProUGUI healthText;
        [Space]
        [Header("Soul Swap Properties")]
        [SerializeField] private Image soulSwapImage;
        [Header("Components")]
        private CharacterController controller;
        [SerializeField] LBRole role;
        private GameManagerRPC gameManagerRPC;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject mainCanvas;
        [Space]
        [Header("Camera")]
        [SerializeField] private new CinemachineFreeLook camera;
        [SerializeField] private LBCameraShake cameraShake;
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
                listener.enabled = false;
                camera.Priority = 0;
                minimapCamera.depth = 0;
            }

            if(!IsOwner) return;
            mainCanvas.SetActive(true);
        }
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            originalStepOffset = controller.stepOffset;
            animator = GetComponent<Animator>();
            cameraShake = GetComponent<LBCameraShake>();

            gameManagerRPC = GameObject.FindObjectOfType<GameManagerRPC>();
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            gameManagerRPC.currentHealth = gameManagerRPC.maxHealth;
        }
        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            UpdateHealth(gameManagerRPC.currentHealth);
            HandleSoulSwapCooldownServerRpc();
        }

        void FixedUpdate()
        {
            InvokeRepeating(nameof(ApplyBurningCoroutine), 0f, GameManagerRPC.Instance.burningInterval);
        }

        private void LateUpdate()
        {
            HandleMovement();
            HandleSoulSwapInput();
        }

        #region Soul Swap Input
        private void HandleSoulSwapInput()
        {
            if (!IsLocalPlayer) return;
            if (soulSwapInput.action.WasPerformedThisFrame() || GameManagerRPC.Instance.IsSoulSwapEnabled)
            {
                SwapSouls();
            }
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

        #region Collision
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                if (other.TryGetComponent<NetworkObject>(out var checkpoint))
                {
                    GameManagerRPC.Instance.SetCheckpoint(checkpoint);
                    Debug.Log($"Checkpoint set to {checkpoint.NetworkObjectId}");
                    other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
                }
            }
            if (other.CompareTag("Platform"))
            {
                var platformNetworkObject = other.gameObject.GetComponent<NetworkObject>();
                var playerNetworkObject = gameObject.GetComponent<NetworkObject>();

                if (NetworkManager.Singleton.IsServer) { platformNetworkObject.ChangeOwnership(playerNetworkObject.OwnerClientId); }

                platformNetworkObject.TrySetParent(playerNetworkObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Lava"))
            {
                TakeDamage(2);
            }
            if (other.CompareTag("Aqua Totem"))
            {
                Heal(2);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Platform"))
            {
                var platformNetworkObject = other.gameObject.GetComponent<NetworkObject>();
                var playerNetworkObject = this.gameObject.GetComponent<NetworkObject>();

                platformNetworkObject.TryRemoveParent(playerNetworkObject);

                if (NetworkManager.Singleton.IsServer) { platformNetworkObject.ChangeOwnership(platformNetworkObject.OwnerClientId); }
            }
        }
        #endregion

        #region Health System
        public void Heal(int amount)
        {
            GameManagerRPC.Instance.currentHealth += amount;
            if (GameManagerRPC.Instance.currentHealth > GameManagerRPC.Instance.maxHealth)
            {
                GameManagerRPC.Instance.currentHealth = GameManagerRPC.Instance.maxHealth;
            }
        }

        public void TakeDamage(int damage)
        {
            GameManagerRPC.Instance.currentHealth -= damage;
            cameraShake.ShakeCamera();
        }

        public void UpdateHealth(int health)
        {
            GameManagerRPC.Instance.currentHealth = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Burning DOT
        IEnumerator ApplyBurningCoroutine()
        {
            if (GameManagerRPC.Instance.isDead) yield break;
            yield return new WaitForSeconds(GameManagerRPC.Instance.burningInterval);
            TakeDamage(GameManagerRPC.Instance.burningDamage);
        }
        #endregion

        #region Death System
        public void Die()
        {
            if (!IsOwner)
            {
                GameManagerRPC.Instance.isDead = true;

                SetPlayerDieServerRpc(GameManagerRPC.Instance.isDead);
            }
            else
            {
                GameManagerRPC.Instance.isDead = true;

                SetPlayerDieServerRpc(GameManagerRPC.Instance.isDead);
            }

            StartCoroutine(RespawnCoroutine());
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerDieServerRpc(bool isDead)
        {
            GameManagerRPC.Instance.isDead = isDead;
        }

        IEnumerator RespawnCoroutine()
        {
            animator.SetTrigger("IsDead");
            yield return new WaitForSeconds(5f);

            // Respawn
            if (IsOwner)
            {
                transform.position = GameManagerRPC.Instance.GetRespawnPosition();
                GameManagerRPC.Instance.currentHealth = GameManagerRPC.Instance.maxHealth;
            }
            else
            {
                transform.position = GameManagerRPC.Instance.GetRespawnPosition();
                SetPlayerHealth(100);
            }
            animator.SetTrigger("IsAlive");
            GameManagerRPC.Instance.isDead = false;

            SetPlayerDieServerRpc(GameManagerRPC.Instance.isDead);
        }

        public void SetPlayerHealth(int health)
        {
            GameManagerRPC.Instance.currentHealth = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Soul Swap
        public void SwapSouls()
        {
            if (!IsOwner)
            {
                if (GameManagerRPC.Instance.soulSwapCooldown <= 0f)
                {
                    SwapSoulsServerRpc();
                    GameManagerRPC.Instance.soulSwapCooldown = 30f;

                    SetSoulSwapCooldownServerRpc(GameManagerRPC.Instance.soulSwapCooldown);
                }
            }
            else
            {
                if (GameManagerRPC.Instance.soulSwapCooldown <= 0f)
                {
                    SwapSoulsServerRpc();
                    GameManagerRPC.Instance.soulSwapCooldown = 30f;

                    SetSoulSwapCooldownServerRpc(GameManagerRPC.Instance.soulSwapCooldown);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetSoulSwapCooldownServerRpc(float cooldown)
        {
            GameManagerRPC.Instance.soulSwapCooldown = cooldown;
        }


        [ServerRpc(RequireOwnership = false)]
        public void SwapSoulsServerRpc()
        {
            if(GameManagerRPC.Instance.IsSoulSwapEnabled) return;
            GameManagerRPC.Instance.IsSoulSwapEnabled = true;
            animator.SetBool("IsSoulSwapEnabled", true);
            role.SwapCharacterModelRpc();
            AudioSource.PlayClipAtPoint(soulSwapSound, transform.position);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HandleSoulSwapCooldownServerRpc()
        {
            if (GameManagerRPC.Instance.soulSwapCooldown > 0f)
            {
                GameManagerRPC.Instance.soulSwapCooldown -= Time.deltaTime;
                soulSwapImage.fillAmount = GameManagerRPC.Instance.soulSwapCooldown / 30f;
            }
            if (GameManagerRPC.Instance.soulSwapCooldown < 0f)
            {
                GameManagerRPC.Instance.soulSwapCooldown = 0f;
                soulSwapImage.fillAmount = 0f;
                role.ResetCharacterModelRpc();
                animator.SetBool("IsSoulSwapEnabled", false);
                GameManagerRPC.Instance.IsSoulSwapEnabled = false;
            }
        }
        #endregion
    }
}