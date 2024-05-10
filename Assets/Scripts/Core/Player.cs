using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.Managers;
using Assets.Scripts.Core;
using System.Threading.Tasks;
namespace LB.Character
{
    public class Player : NetworkBehaviour
    {
        [Header("Input Action References")]
        [SerializeField] private InputActionReference movementInput;
        [SerializeField] private InputActionReference jumpInput;
        [SerializeField] private InputActionReference soulSwapInput;
        [Space]
        [Header("Movement Properties")]
        private float movementSpeed = 12f;
        private float jumpHeight = 14f;
        private float jumpButtonGracePeriod;
        private float ySpeed;
        private float originalStepOffset;
        private float? lastGroundedTime;
        private float? jumpButtonPressedTime;
        private bool isGrounded;
        private bool isJumping;
        private float jumpHorizontalSpeed = 1f;
        [Space]
        [Header("Health System")]
        public bool isPlayerDead = false;
        public int currentHealth;
        public int maxHealth = 100;
        private bool isHealingActive = false;
        [SerializeField] private float healTickInterval = 2f;
        [SerializeField] private float healTimer = 0f;
        [Space]
        [SerializeField] private TextMeshProUGUI healthText;
        [Space]
        [Header("Damage System")]
        private bool isDamageFromLavaActive = false;
        [SerializeField] private float damageTickInterval = 3f;
        [SerializeField] private float damageTimer = 0f;
        [Space]
        [Header("Soul Swap Properties")]
        [SerializeField] private Image soulSwapImage;
        [SerializeField] private bool isSoulSwapActive = false;
        private bool isSoulSwapInCooldown = false;
        [Header("Components")]
        private CharacterController controller;
        [SerializeField] LBRole role;
        private GameManagerRPC gameManagerRPC;
        private LBAudioManager audioManager;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject mainCanvas;
        [SerializeField] private GameObject fadeCanvas;
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

            if (!IsOwner) return;
            mainCanvas.SetActive(true);
            fadeCanvas.SetActive(false);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            originalStepOffset = controller.stepOffset;
            animator = GetComponent<Animator>();
            cameraShake = GetComponent<LBCameraShake>();

            gameManagerRPC = FindObjectOfType<GameManagerRPC>();
            audioManager = FindObjectOfType<LBAudioManager>();
        }

        void Start()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            UpdateHealth(currentHealth);
        }

        private void LateUpdate()
        {
            HandleMovement();
            HandleSoulSwapInput();

            if (Input.GetKeyDown(KeyCode.R))
            {
                Die();
            }
        }


        #region Movement
        /// <summary>
        /// Handles the player's movement.
        /// </summary>
        private void HandleMovement()
        {
            if (!IsLocalPlayer) return;
            if (isPlayerDead) return;
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

        #region Soul Swap Input
        private void HandleSoulSwapInput()
        {
            if (soulSwapInput.action.WasPerformedThisFrame())
            {
                if (isPlayerDead) return;
                ActivateSkill();
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
                    gameManagerRPC.SetCheckpoint(checkpoint);
                    Debug.Log($"Checkpoint set to {checkpoint.NetworkObjectId}");
                    other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Lava"))
            {
                isDamageFromLavaActive = true;
                ApplyDamageOverTime();
            }

            if (other.CompareTag("Aqua Totem"))
            {
                isHealingActive = true;
                ApplyHealOverTime();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Platform"))
            {
                NetworkObject platform = other.GetComponent<NetworkObject>();
                if (platform != null)
                {
                    platform.transform.SetParent(null);
                }
            }
            if (other.CompareTag("Aqua Totem"))
            {
                isHealingActive = false;
            }
            if (other.CompareTag("Lava"))
            {
                isDamageFromLavaActive = false;
            }
        }
        #endregion

        #region Health System
        public void ApplyHealOverTime()
        {
            if (isHealingActive)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healTickInterval)
                {
                    Heal(2);
                    healTimer = 0f;
                }
            }
        }

        public void Heal(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHealth(currentHealth);
        }
        #endregion

        #region Damage System
        public void ApplyDamageOverTime()
        {
            if (isDamageFromLavaActive)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageTickInterval)
                {
                    TakeDamage(4);
                    damageTimer = 0f;
                }
            }
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            cameraShake.ShakeCamera();
            audioManager.PlaySound(hurtSound);
            if (currentHealth <= 0)
            {
                Die();
            }
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHealth(currentHealth);
        }

        public void UpdateHealth(int health)
        {
            if (!IsLocalPlayer) return;
            currentHealth = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Death System
        public async void Die()
        {
            animator.SetTrigger("IsDead");
            isPlayerDead = true;
            await Task.Delay(2500);
            fadeCanvas.SetActive(true);
            fadeCanvas.GetComponent<Animator>().Play("Fade_Out");
            mainCanvas.SetActive(false);
            SetDespawnPosition();
        }

        public async void SetDespawnPosition()
        {
            await Task.Delay(2500);
            transform.position = gameManagerRPC.GetRespawnPosition();
            currentHealth = maxHealth;
            animator.SetTrigger("IsAlive");
            mainCanvas.SetActive(true);
            await Task.Delay(2500);
            fadeCanvas.GetComponent<Animator>().Play("Fade_In");
            fadeCanvas.SetActive(false);
            isPlayerDead = false;
        }

        public void SetPlayerHealth(int health)
        {
            currentHealth = health;
            healthText.SetText($"Health: {health}");
        }
        #endregion

        #region Soul Swap
        public async void ActivateSkill()
        {
            if (isSoulSwapInCooldown) return;
            animator.SetBool("IsSoulSwapEnabled", true);
            await Task.Delay(1000);
            isSoulSwapActive = true;
            StartCoroutine(HandleCooldown());
            SwapPlayerModel();
        }

        private IEnumerator HandleCooldown()
        {
            isSoulSwapInCooldown = true;
            soulSwapImage.fillAmount = 0;
            float time = 0;
            while (time < gameManagerRPC.soulSwapCooldown)
            {
                time += Time.deltaTime;
                soulSwapImage.fillAmount = time / gameManagerRPC.soulSwapCooldown;
                yield return new WaitForSeconds(gameManagerRPC.soulSwapCooldown);
            }
            isSoulSwapInCooldown = false;
        }

        public void SwapPlayerModel()
        {
            audioManager.PlaySound(soulSwapSound);
            role.SwapCharacterModelRpc();
        }

        public void ResetPlayerModel()
        {
            audioManager.PlaySound(soulSwapSound);
            role.ResetCharacterModelRpc();
        }
        #endregion
    }
}