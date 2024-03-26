using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class LBHealthSystem : MonoBehaviour
{
    [Header("Properties")]
    [Tooltip("Time in seconds for timer countdown")]
    [SerializeField] private float heatTimer;
    [Tooltip("Rate at which heat gauge fills")]
    [SerializeField] private float heatGaugeFillRate;
    [Tooltip("Damage dealt per tick by Burning effect")]
    [SerializeField] private float burningDamagePerTick;
    [Tooltip("HP regeneration per tick near Aqua Totem")]
    [SerializeField] private float hpRegenerationPerTick;
    [Header("UI")]
    [SerializeField] private GameObject heatBarGauge;
    [SerializeField] private Image heatBarGaugeFill;
    [SerializeField] private GameObject healthBarGauge;
    [SerializeField] private Image healthBarGaugeFill;
    [SerializeField] private TMP_Text countdownTextTimer;
    [SerializeField] private GameObject burningVignetteEffectImage;
    [Header("Flags")]
    private float currentHeatLevel;
    private bool isHeatGaugeActive;
    private bool isPlayerBurning;
    private float playerMaxHP = 100f;
    private float currentHealth;
    private bool isInsideAquaTotem;

    private void Start()
    {
        currentHealth = playerMaxHP;
        currentHeatLevel = 0f;
        isHeatGaugeActive = false;
        isPlayerBurning = false;
        heatBarGauge.SetActive(false);
        burningVignetteEffectImage.SetActive(false);
    }

    private void Update()
    {
        if (!isHeatGaugeActive)
        {
            heatTimer -= Time.deltaTime;
            int remainingSeconds = Mathf.FloorToInt(heatTimer);
            string formattedTime = string.Format("{0:00}:{1:00}", remainingSeconds / 60, remainingSeconds % 60);

            countdownTextTimer.SetText(formattedTime);
            if (Mathf.Clamp01(heatTimer) <= 0f)
            {
                countdownTextTimer.enabled = false;
                isHeatGaugeActive = true;
                heatBarGauge.SetActive(true);
            }
        }
        else
        {
            if (!isInsideAquaTotem && currentHeatLevel < 1f)
            {
                currentHeatLevel += heatGaugeFillRate * Time.deltaTime;
                heatBarGaugeFill.fillAmount = currentHeatLevel;
                Debug.Log($"Current Heat Level: {currentHeatLevel}");
            }
            else
            {
                isPlayerBurning = true;
            }

            if (isPlayerBurning)
            {
                TakeDamage(burningDamagePerTick);
                burningVignetteEffectImage.SetActive(true);
                if (currentHealth <= 0f)
                {
                    Debug.Log($"Player is dead");
                }
            }
        }

        if (isInsideAquaTotem && currentHeatLevel <= 0f)
        {
            heatBarGauge.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Aqua Totem"))
        {
            isInsideAquaTotem = true;
            if (isPlayerBurning && currentHeatLevel > 0f)
            {
                isPlayerBurning = false;
                currentHeatLevel = 0f;
                heatBarGaugeFill.fillAmount = 0f;
                burningVignetteEffectImage.SetActive(false);
            }
            RegainHealth(hpRegenerationPerTick);
            if (currentHeatLevel > 1f)
            {
                currentHeatLevel -= heatGaugeFillRate * Time.deltaTime;
                heatBarGaugeFill.fillAmount = currentHeatLevel;
            }
        }
        else
        {
            isInsideAquaTotem = false;
            if (isHeatGaugeActive && currentHeatLevel > 0f)
            {
                currentHeatLevel -= heatGaugeFillRate * Time.deltaTime;
                heatBarGaugeFill.fillAmount = currentHeatLevel;
            }
        }
    }

    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBarGaugeFill.fillAmount = currentHealth / playerMaxHP;
        currentHealth = Mathf.Clamp(currentHealth, 0, playerMaxHP);
    }

    private void RegainHealth(float regainHealth)
    {
        currentHealth += regainHealth;
        healthBarGaugeFill.fillAmount = currentHealth / playerMaxHP;
        currentHealth = Mathf.Clamp(currentHealth, 0, playerMaxHP);
    }
}
