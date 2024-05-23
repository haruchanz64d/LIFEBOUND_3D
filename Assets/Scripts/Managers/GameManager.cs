using UnityEngine;
using Unity.Netcode;
using Assets.Scripts.Core;
using TMPro;
using System.Collections;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager: NetworkBehaviour
{
    private GameObject networkManagerGameObject;
    public static GameManager Instance { get; private set; }
    private HostManager hostManager;
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
    [SerializeField] private int collectionGoal = 14;
    private int maxHeartsScatteredOnTheMap = 20;
    [SerializeField] private int currentCollectionCount;
    public int CurrentCollectionCount => currentCollectionCount;
    [SerializeField] private bool isCollectionGoalReached;
    public bool IsCollectionGoalReached => isCollectionGoalReached;
    [SerializeField] private TMP_Text collectionText;
    [SerializeField] private TMP_Text announcementText;
    private AudioClip collectionReached;
    [SerializeField] private GameObject portalObject;

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

        hostManager = GameObject.FindObjectOfType<HostManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isHeatWaveActivated = true;
        isCountdownActivated = true;
        networkManagerGameObject = GameObject.FindGameObjectWithTag("NetworkManager");
        if (!IsServer) return;
    }


    private void Start()
    {
        if (IsServer && IsClient)
        {
            currentCollectionCount = 0;
            collectionText.text = "Collection Progress: 0%";
            announcementText.text = "";
        }
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

    public void DisconnectAllPlayers()
    {
        StartCoroutine(DisconnectAllPlayersToMainMenu());
    }

    public IEnumerator DisconnectAllPlayersToMainMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.Shutdown();
                hostManager.OnClientDisconnect(hostManager.ClientData.Keys.GetEnumerator().Current);
                NetworkManager.Singleton.ConnectionApprovalCallback -= hostManager.ApprovalCheck;
            }
            if (NetworkManager.Singleton.IsClient)
            {
                SceneManager.LoadScene(1);
            }
        }

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(1);
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
        if (IsServer)
        {
            currentCollectionCount++;
            int percentage = Mathf.Min((currentCollectionCount * 100) / maxHeartsScatteredOnTheMap, 100);
            UpdateCollectionCountClientRpc(percentage);
            int seventyPercentThreshold = Mathf.CeilToInt(maxHeartsScatteredOnTheMap * 0.7f);

            if (currentCollectionCount >= seventyPercentThreshold && !isCollectionGoalReached)
            {
                isCollectionGoalReached = true;
                StartCoroutine(AnnounceCollectionGoalReached());
            }
        }
    }

    [ClientRpc]
    private void UpdateCollectionCountClientRpc(int percentage)
    {
        collectionText.text = $"Collection Progress: {percentage}%";
    }

    private IEnumerator AnnounceCollectionGoalReached()
    {
        AudioManager.Instance.PlaySound(collectionReached);
        AnnounceCollectionGoalReachedClientRpc();
        yield return new WaitForSeconds(3f);
        ClearAnnouncementClientRpc();
    }

    [ClientRpc]
    private void AnnounceCollectionGoalReachedClientRpc()
    {
        announcementText.text = "Collection Goal Reached!";
    }

    [ClientRpc]
    private void ClearAnnouncementClientRpc()
    {
        announcementText.text = string.Empty;
    }

    #endregion
}