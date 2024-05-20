using UnityEngine;
using Unity.Netcode;
using Assets.Scripts.Core;
using TMPro;
using System.Collections;

public class GameManager: NetworkBehaviour
{
    [SerializeField] private GameObject networkManagerGameObject;
    public static GameManager Instance { get; private set; }

    [Header("Soul Swap")]
    private float soulSwapCooldown = 30f;
    public float SoulSwapCooldown => soulSwapCooldown;

    [Header("Heat Wave")]
    private bool isHeatWaveActivated;
    private bool isCountdownActivated;
    private int heatWaveDamage = 1;
    private float heatTickInterval = 10f;
    private float heatWaveTimer = 0f;

    [Header("Collection System")]
    [SerializeField] private int collectionGoal = 20;
    [SerializeField] private int currentCollectionCount;
    public int CurrentCollectionCount => currentCollectionCount;
    [SerializeField] private bool isCollectionGoalReached;
    public bool IsCollectionGoalReached => isCollectionGoalReached;
    [SerializeField] private TMP_Text collectionText;
    [SerializeField] private TMP_Text announcementText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isHeatWaveActivated = true;
        isCountdownActivated = true;
        networkManagerGameObject = GameObject.FindGameObjectWithTag("NetworkManager");

        if (!IsServer) return;
    }

    private void Update()
    {
        if (IsServer)
        {
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
    }

    private void LateUpdate()
    {
        if (IsServer)
        {
            if (isCountdownActivated)
            {
                StartCountdownServerRpc();
            }
        }
        CheckForPlayerDeathStateServerRpc();
    }

    #region Countdown
    [ServerRpc(RequireOwnership = false)]
    private void StartCountdownServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out CountdownTimer timer))
            {
                timer.UpdateCountdownTimerServerRpc();
            }
        }
    }

    [ClientRpc]
    public void StartCountdownClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out CountdownTimer timer))
            {
                timer.UpdateCountdownTimer();
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void ApplyCountdownEndServerRpc()
    {
        Debug.Log($"Countdown has ended...");
        ApplyCountdownEndClientRpc();
    }

    [ClientRpc]
    public void ApplyCountdownEndClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                health.ForceKillPlayer();
            }
        }
    }

    #endregion

    public void DisconnectAllPlayers(ulong clientId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if(player.TryGetComponent(out NetworkObject networkObject))
            {
                if (networkObject.OwnerClientId == clientId)
                {
                    DisconnectAllPlayersToMainMenu();
                }
            }
        }
    }

    public void DisconnectAllPlayersToMainMenu()
    {
        if (IsClient) { UnityEngine.SceneManagement.SceneManager.LoadScene(1); }
        Destroy(networkManagerGameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    #region Death System
    [ServerRpc(RequireOwnership = false)]
    private void CheckForPlayerDeathStateServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int deadPlayerCount = 0;

        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                if (health.IsPlayerDead)
                {
                    deadPlayerCount++;
                }
            }
        }

        if (deadPlayerCount > 0)
        {
            ApplyDeathStateClientRpc();
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
                if (!health.IsPlayerDead)
                {
                    health.ForceKillPlayer();
                }
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

    #region Lava Kill Zone
    [ServerRpc(RequireOwnership = false)]
    public void KillAllPlayersDueToLavaServerRpc()
    {
        ApplyLavaDeathClientRpc();
    }

    [ClientRpc]
    private void ApplyLavaDeathClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out HealthSystem health))
            {
                health.ForceKillPlayer();
            }
        }
    }
    #endregion

    #region Collection System
    public void UpdateCollectionCount()
    {
        currentCollectionCount++;
        float percentage = (currentCollectionCount / (float)collectionGoal) * 100f;
        collectionText.text = $"Hearts Collected: {percentage:F2}%";
        if (currentCollectionCount >= collectionGoal)
        {
            isCollectionGoalReached = true;
            StartCoroutine(AnnounceCollectionGoalReached());
        }
    }

    private IEnumerator AnnounceCollectionGoalReached()
    {
        announcementText.text = "Collection Goal Reached!";
        yield return new WaitForSeconds(3f);
        announcementText.text = string.Empty;
    }
    #endregion
}