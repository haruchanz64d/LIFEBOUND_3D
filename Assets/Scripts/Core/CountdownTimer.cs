using Assets.Scripts.Core;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CountdownTimer : NetworkBehaviour
{
    [Header("Countdown")]
    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(5 * 60f);
    [SerializeField] private TMP_Text countdownText;
    private float elapsedMinutes = 0f;
    private HealthSystem healthSystem;

    private bool isTimerRunOut = false;
    public bool IsTimerRunOut
    {
        get => isTimerRunOut;
        set => isTimerRunOut = value;
    }
    private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isTimerRunOut = false;
        gameManager = GameManager.Instance;

        if (IsClient)
        {
            UpdateCountdownText();
        }

        if (IsServer)
        {
            // Initialize the countdown timer on the server
            countdownTimer.Value = 5 * 60f;
        }

        // Subscribe to the NetworkVariable value change event
        countdownTimer.OnValueChanged += OnCountdownTimerChanged;

        UpdateCountdownText();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed
        countdownTimer.OnValueChanged -= OnCountdownTimerChanged;
    }

    private void OnCountdownTimerChanged(float oldValue, float newValue)
    {
        // Update the countdown text when the NetworkVariable value changes
        UpdateCountdownText();
    }

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCountdownTimerServerRpc()
    {
        UpdateCountdownTimer();
    }

    [ClientRpc]
    public void UpdateCountdownTimerClientRpc()
    {
        UpdateCountdownTimer();
    }

    public void UpdateCountdownTimer()
    {
        if (healthSystem.IsPlayerDead) return;
        if (countdownTimer.Value > 0)
        {
            countdownTimer.Value -= Time.deltaTime;
            elapsedMinutes += Time.deltaTime / 60f;
            if (countdownTimer.Value <= 0)
            {
                countdownTimer.Value = 0;
                isTimerRunOut = true;
                gameManager.ApplyCountdownEndServerRpc();
            }
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            int minutes = Mathf.FloorToInt(countdownTimer.Value / 60);
            int seconds = Mathf.FloorToInt(countdownTimer.Value % 60);
            countdownText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
        }
    }
}
