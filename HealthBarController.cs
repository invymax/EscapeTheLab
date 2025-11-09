using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image frontFill; // текущая мгновенная полоска
    [SerializeField] private Image backFill;  // «чип» (отстаёт при уроне)
    [SerializeField] private TMPro.TextMeshProUGUI hpText; // необязательно

    [Header("Appearance")]
    [SerializeField] private Gradient colorOverHealth; // 1.0 = зелёный, 0.0 = красный
    [SerializeField] private Color chipDamageColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color chipHealColor = new Color(0.3f, 1f, 0.3f);

    [Header("Animation")]
    [SerializeField] private float lerpSpeed = 8f;       // скорость плавности основной полоски
    [SerializeField] private float chipLerpSpeed = 3f;   // скорость «чипа»
    [SerializeField] private float lowHpThreshold = 0.2f; // <20% — мигаем
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseAmplitude = 0.05f;

    private float targetFill;   // целевое значение [0..1]
    private float backTarget;   // куда должен прийти чип
    private float frontCurrent; // текущее значение front
    private float backCurrent;  // текущее значение back
    private bool pulsing;

    private void Reset()
    {
        // Авто-поиск, если забыли проставить в инспекторе
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void Awake()
    {
        if (!playerHealth)
            playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            targetFill = playerHealth.CurrentHealth / playerHealth.MaxHealth;
            frontCurrent = backCurrent = targetFill;
            ApplyInstant(frontCurrent);
        }
    }

    private void Update()
    {
        // Плавная анимация front → target
        frontCurrent = Mathf.Lerp(frontCurrent, targetFill, Time.deltaTime * lerpSpeed);

        // Чип ведёт себя по-разному: при уроне — отстаёт вниз, при лечении — отстаёт вверх.
        if (backCurrent > targetFill)
        {
            // Урон: чип тянется вниз
            backCurrent = Mathf.Lerp(backCurrent, targetFill, Time.deltaTime * chipLerpSpeed);
            backFill.color = chipDamageColor;
        }
        else
        {
            // Хил: чип тянется вверх (можно быстрее/медленнее)
            backCurrent = Mathf.Lerp(backCurrent, targetFill, Time.deltaTime * (chipLerpSpeed * 0.8f));
            backFill.color = chipHealColor;
        }

        ApplyInstant(frontCurrent, backCurrent);
        UpdatePulse(frontCurrent);

        if (hpText != null && playerHealth != null)
        {
            hpText.text = $"{Mathf.CeilToInt(playerHealth.CurrentHealth)}/{Mathf.CeilToInt(playerHealth.MaxHealth)}";
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        targetFill = Mathf.Clamp01(current / max);
        // Обновляем цвет основной полоски от градиента
        if (frontFill != null)
            frontFill.color = colorOverHealth.Evaluate(targetFill);
    }

    private void ApplyInstant(float front)
    {
        if (frontFill) frontFill.fillAmount = front;
        if (backFill) backFill.fillAmount = front;
    }

    private void ApplyInstant(float front, float back)
    {
        if (frontFill) frontFill.fillAmount = front;
        if (backFill) backFill.fillAmount = back;
    }

    private void UpdatePulse(float fill)
    {
        bool low = fill <= lowHpThreshold;
        if (!low)
        {
            if (pulsing)
            {
                pulsing = false;
                transform.localScale = Vector3.one;
            }
            return;
        }

        pulsing = true;
        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = new Vector3(s, s, 1f);
    }
}