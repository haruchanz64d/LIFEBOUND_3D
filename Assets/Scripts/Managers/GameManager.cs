using UnityEngine;
using Unity.Netcode;
using LB.Character;
using Assets.Scripts.Core;
using System;
using TMPro;


public class GameManager: NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Checkpoint")]
    [SerializeField] private GameObject defaultSpawn;
    public GameObject DefaultSpawn => defaultSpawn;
    private Vector3 lastInteractedCheckpointPosition;
    public Vector3 LastInteractedCheckpointPosition
    {
        get => lastInteractedCheckpointPosition;
        set => lastInteractedCheckpointPosition = value;
    }

    [Header("Countdown")]
    private float countdownTimer = 5 * 60;
    [SerializeField] private TMP_Text countdownText;
    private float elapsedMinutes = 0f;
    [Header("Soul Swap")]
    private float soulSwapCooldown = 30f;
    public float SoulSwapCooldown => soulSwapCooldown;

    [Header("Heat Wave")]
    private bool isHeatWaveActivated;
    private int heatWaveDamage = 1;
    private float heatTickInterval = 10f;
    private float heatWaveTimer = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isHeatWaveActivated = true;
    }
    private void Awake()
    {
        if (!IsServer) return;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    private void Update()
    {
        if (IsServer)
        {
            if (countdownTimer > 0)
            {
                countdownTimer -= Time.deltaTime;
                elapsedMinutes += Time.deltaTime / 60f;
                if (countdownTimer <= 0)
                {
                    countdownTimer = 0;
                    ApplyCountdownEndServerRpc();
                }

                if (Mathf.FloorToInt(elapsedMinutes) > Mathf.FloorToInt(elapsedMinutes - Time.deltaTime / 60f))
                {
                    heatWaveDamage += 1;
                }
            }

            if (isHeatWaveActivated)
            {
                heatWaveTimer += Time.deltaTime;
                if (heatWaveTimer >= heatTickInterval)
                {
                    ApplyHeatwaveDamageServerRpc();
                    heatWaveTimer = 0f;
                }
            }
        }

        UpdateCountdownText();
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            int minutes = Mathf.FloorToInt(countdownTimer / 60);
            int seconds = Mathf.FloorToInt(countdownTimer % 60);
            countdownText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
        }
    }

    #region Countdown
    [ServerRpc(RequireOwnership = false)]
    private void ApplyCountdownEndServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                ApplyCountdownEndClientRpc();
            }
        }
    }

    [ClientRpc]
    public void ApplyCountdownEndClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                health.KillPlayer();
            }
        }
    }
    #endregion

    private void LateUpdate()
    {
        CheckForPlayerDeathStateServerRpc();
    }

    public void DisconnectAllPlayers()
    {
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    #region Death System
    [ServerRpc(RequireOwnership = false)]
    private void CheckForPlayerDeathStateServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                if (health.IsPlayerDead)
                {
                    ApplyDeathStateClientRpc();
                }
            }
        }
    }

    [ClientRpc]
    public void ApplyDeathStateClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                health.KillPlayer();
            }
        }
    }
    #endregion

    #region RPCs (Heat Wave)
    [ServerRpc(RequireOwnership = false)]
    private void ApplyHeatwaveDamageServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if(player.TryGetComponent(out HealthSystem health))
            {
                ApplyHeatwaveDamageClientRpc(heatWaveDamage);
            }
        }
    }

    [ClientRpc]
    public void ApplyHeatwaveDamageClientRpc(int damage)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(damage);
            }
        }
    }
    #endregion
}