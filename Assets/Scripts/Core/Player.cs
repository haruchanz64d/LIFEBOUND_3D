using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
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
        [Header("Properties")]
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
        [Header("Components")]
        private CharacterController controller;
        private Animator animator;
        private LBCanvasManager canvas;
        private LBGameManager gameManager;
        [Space]
        [Header("Camera")]
        [SerializeField] private new CinemachineFreeLook camera;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private AudioListener listener;
        [SerializeField] private Transform cameraTransform;
        [Space]
        [Header("Spawn")]
        public bool isDead = false;
        private Vector3 lastCheckpointInteracted;
        [Space]
        [Header("Health")]
        private float health = 100f;
        public float GetCurrentHP => health;
        [Space]
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        [Space]
        [Header("Skills")]
        [SerializeField] private Image soulSwapImage;
        [SerializeField] private float soulSwapCooldown = 10f;
        private bool isCooldown = false;
        private bool isSoulSwapSkillActivated;
        public bool IsSoulSwapSkillActivated => isSoulSwapSkillActivated;
        [SerializeField] private Transform otherPlayerPosition;
        private Vector3 currentPosition;
        public Vector3 GetCurrentPosition(Vector3 position)
        {
            return currentPosition;
        }

        public void SetCurrentPosition(Vector3 position)
        {
            currentPosition = position;
        }

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
            canvas = FindObjectOfType<LBCanvasManager>();
            controller = GetComponent<CharacterController>();
            originalStepOffset = controller.stepOffset;
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        private void Update()
        {
            if (canvas.GetGameplayPaused) return;
            if (!IsLocalPlayer) return;
            if (isDead) return;
        }

        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// </summary>

        private void LateUpdate()
        {
            if (canvas.GetGameplayPaused) return;
            if (!IsLocalPlayer) return;
            if (isDead) return;
            if (soulSwapInput.action.WasPressedThisFrame())
            {
                HandleSoulSwap();
            }
            HandleMovement();
        }

        /// <summary>
        /// Handles the player's movement.
        /// </summary>
        private void HandleMovement()
        {
            if (isDead) return;
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

        /// <summary>
        /// Called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="other"></param>
        /// 
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Checkpoint"))
            {
                Debug.Log($"Checkpoint located at X: {other.transform.position.x}, Y: {other.transform.position.y}, Z: {other.transform.position.z}");
                lastCheckpointInteracted = other.gameObject.transform.position;
                other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
                SetCheckpointClientRpc(lastCheckpointInteracted);
            }
        }

        [ClientRpc]
        public void SetCheckpointClientRpc(Vector3 checkpointPosition)
        {
            LBGameManager.Instance.SetLastCheckpointServerRpc(checkpointPosition);
        }

        /// <summary>
        /// OnTriggerStay is called once per frame for every Collider other that is touching the trigger.
        /// </summary>
        /// <param name="hit"></param>
        private void OnTriggerStay(Collider hit)
        {
            if (hit.gameObject.CompareTag("Lava"))
            {
                TakeDamage(2f);

                if (Mathf.Clamp(health, 0, 100) <= 0f)
                {
                    Die();
                }
            }
            if (hit.gameObject.CompareTag("Aqua Totem"))
            {
                RegainHealth(1f);
            }
        }

        /// <summary>
        /// Takes damage from the player character.
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(float damage)
        {
            health -= Mathf.Clamp(damage, 0f, 100f);

            healthText.SetText($"Health: {health}");
        }

        /// <summary>
        /// Regains health for the player character.
        /// </summary>
        /// <param name="regainHealth"></param>
        private void RegainHealth(float regainHealth)
        {
            health = Mathf.Clamp(health + regainHealth, 0f, 100f);
            healthText.SetText($"Health: {health}");
        }

        /// <summary>
        /// Kills the player character.
        /// </summary>
        public void Die()
        {
            DieClientRpc();
        }

        /// <summary>
        /// Handles the player's death via a client RPC.
        /// </summary>
        [ClientRpc]
        private void DieClientRpc()
        {
            animator.SetTrigger("IsDead");
            isDead = true;
        }

        /// <summary>
        /// Handles the soul swap skill.
        /// </summary>
        private void HandleSoulSwap()
        {
            isSoulSwapSkillActivated = true;
            isCooldown = true;
            if (isCooldown)
            {
                soulSwapImage.fillAmount -= 1 / soulSwapCooldown * Time.deltaTime;

                if (soulSwapImage.fillAmount <= 0)
                {
                    isCooldown = false;
                    soulSwapImage.fillAmount = 1;
                    isSoulSwapSkillActivated = false;
                }
            }
        }
    }
}