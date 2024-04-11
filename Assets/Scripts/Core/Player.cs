using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine.UI;
namespace LB.Character
{
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
        [Space]
        [Header("Camera")]
        [SerializeField] private new CinemachineFreeLook camera;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private AudioListener listener;
        [SerializeField] private Transform cameraTransform;
        [Space]
        [Header("Spawn")]
        private Transform originalSpawnPoint;
        private bool isPlayerDead = false;
        private Vector3 lastCheckpointInteracted;
        [Space]
        [Header("Heatwave - Mechanic")]
        private float heatwaveDamage = 2f;
        private float heatwaveInterval = 10f;
        private float heatwaveTimer = 0f;
        [Space]
        [Header("Health - Mechanic")]
        private float health = 100f;
        [Space]
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        [Space]
        [Header("Skills")]
        [SerializeField] private Image soulSwapImage;
        [SerializeField] private float soulSwapCooldown = 10f;
        private bool isCooldown = false;
        private bool isSoulSwapping = false;
        private bool isSoulSwapActive = false;
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


        private void Awake()
        {
            canvas = FindObjectOfType<LBCanvasManager>();
            controller = GetComponent<CharacterController>();
            originalStepOffset = controller.stepOffset;
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (canvas.GetGameplayPaused) return;
            if (!IsLocalPlayer) return;
            if (isPlayerDead) StartCoroutine(RespawnPlayer());
            HandleBlazingHeat();
            LBGameManager.Instance.BroadcastPlayerPositionServerRpc(transform.position);
        }

        private void LateUpdate()
        {
            if (canvas.GetGameplayPaused) return;
            if (!IsLocalPlayer) return;
            if (isPlayerDead) return;
            if(soulSwapInput.action.WasPressedThisFrame())
            {
                HandleSoulSwap();
            }
            HandleMovement();
        }

        private void HandleMovement()
        {
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

        private void OnAnimatorMove()
        {
            if (isGrounded)
            {
                Vector3 velocity = animator.deltaPosition;
                velocity.y = ySpeed * Time.deltaTime;

                controller.Move(velocity);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Checkpoint"))
            {
                Debug.Log($"Checkpoint located at X: {other.transform.position.x}, Y: {other.transform.position.y}, Z: {other.transform.position.z}");
                lastCheckpointInteracted = other.gameObject.transform.position;
                other.GetComponent<LBCheckpoint>().OnCheckpointActivated();
            }
        }

        private void OnTriggerStay(Collider hit)
        {
            if (hit.gameObject.CompareTag("Lava"))
            {
                TakeDamage(2f);

                if(health <= 0f)
                {
                    Die();
                }
            }
            if (hit.gameObject.CompareTag("Aqua Totem"))
            {
                RegainHealth(1f);
            }
        }

        public IEnumerator RespawnPlayer()
        {
            yield return new WaitForSeconds(5f);

            Vector3 respawnPosition = lastCheckpointInteracted != Vector3.zero ? lastCheckpointInteracted : originalSpawnPoint.position;
            transform.position = respawnPosition;

            if (lastCheckpointInteracted != Vector3.zero)
            {
                Debug.Log($"Respawned player at checkpoint: {lastCheckpointInteracted}");
            }
            else
            {
                Debug.Log("Respawned player at original spawn point.");
            }

            Debug.Log($"Respawned player at checkpoint: {lastCheckpointInteracted}");

            isPlayerDead = false;
            animator.SetTrigger("IsAlive");
        }

        private void HandleBlazingHeat()
        {
            heatwaveTimer += Time.deltaTime;
            if (heatwaveTimer >= heatwaveInterval)
            {
                TakeDamage(heatwaveDamage);
                heatwaveTimer = 0f;
            }
        }

        private void TakeDamage(float damage)
        {
            health -= Mathf.Clamp(damage, 0f, 100f);

            healthText.SetText($"Health: {health}");
        }

        private void RegainHealth(float regainHealth)
        {
            health = Mathf.Clamp(health + regainHealth, 0f, 100f);
            healthText.SetText($"Health: {health}");
        }

        public void Die()
        {
            isPlayerDead = true;
            animator.Play("Dead", 0, 0);
            
            LBGameManager.Instance.BroadcastPlayerDeathServerRpc(NetworkObjectId);

            if (otherPlayerPosition != null)
            {
                Player otherPlayer = otherPlayerPosition.GetComponent<Player>();
                if (otherPlayer != null)
                {
                    otherPlayer.Die();
                }
            }

            StartCoroutine(RespawnPlayer());
        }

        private void HandleSoulSwap()
        {
            isCooldown = true;
            if(isCooldown)
            {
                soulSwapImage.fillAmount -= 1 / soulSwapCooldown * Time.deltaTime;

                if(soulSwapImage.fillAmount <= 0)
                {
                    isCooldown = false;
                    soulSwapImage.fillAmount = 1;
                }
            }

            if (isSoulSwapActive)
            {
                if (isSoulSwapping)
                {
                    // Swap positions with the other player
                    if (otherPlayerPosition != null)
                    {
                        Player otherPlayer = otherPlayerPosition.GetComponent<Player>();
                        if (otherPlayer != null)
                        {
                            Vector3 tempPosition = transform.position;
                            transform.position = otherPlayer.GetCurrentPosition(transform.position);
                            otherPlayer.SetCurrentPosition(tempPosition);
                        }
                    }
                    else
                    {
                        Debug.Log("No other player found to swap positions with.");
                    }
                }
            }
        }
    }
}