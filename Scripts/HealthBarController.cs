using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image frontFill; // текущая полоска (должна быть сверху)
    [SerializeField] private Image backFill;  // «чип» (отстаёт при уроне)
    [SerializeField] private TextMeshProUGUI hpText; // необязательно

    [Header("Appearance")]
    [SerializeField] private Gradient colorOverHealth; // цвет по % здоровья
    [SerializeField] private Color chipDamageColor = new Color(1f, 0.3f, 0.3f, 0.55f); // прозрачность
    [SerializeField] private Color chipHealColor = new Color(0.3f, 1f, 0.3f, 0.45f);

    [Header("Animation")]
    [SerializeField] private float lerpSpeed = 8f;       // скорость front
    [SerializeField] private float chipLerpSpeed = 3f;   // скорость back (чип)
    [SerializeField] private float colorLerpSpeed = 12f; // скорость плавной смены цвета (опциональная)
    [SerializeField] private float lowHpThreshold = 0.2f; // порог мигания
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseAmplitude = 0.05f;

    private float targetFill;   // целевое значение [0..1]
    private float frontCurrent; // текущее значение front
    private float backCurrent;  // текущее значение back

    private Color currentFrontColor;

    private void Reset()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
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
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        // Гарантируем порядок: frontFill должен быть визуально НАД backFill
        if (backFill != null)
            backFill.transform.SetSiblingIndex(0); // положим назад
        if (frontFill != null)
            frontFill.transform.SetAsLastSibling(); // поставим фронт поверх

        // Если у chip цвета нет альфы — зададим её, чтобы он не закрывал front
        chipDamageColor.a = chipDamageColor.a <= 0f ? 0.55f : chipDamageColor.a;
        chipHealColor.a = chipHealColor.a <= 0f ? 0.45f : chipHealColor.a;
        if (backFill != null) backFill.color = chipDamageColor;

        targetFill = playerHealth != null ? playerHealth.CurrentHealth / playerHealth.MaxHealth : 1f;
        frontCurrent = backCurrent = targetFill;

        // Инициализация цвета
        currentFrontColor = colorOverHealth != null ? colorOverHealth.Evaluate(frontCurrent) : Color.green;
        if (frontFill != null) frontFill.color = currentFrontColor;
    }

    private void Update()
    {
        // Плавное смещение front к target
        frontCurrent = Mathf.Lerp(frontCurrent, targetFill, Time.deltaTime * lerpSpeed);

        // Чип: если back текущий больше target (урон) — отстаёт вниз; иначе (хил) тянется вверх
        if (backCurrent > targetFill)
        {
            backCurrent = Mathf.Lerp(backCurrent, targetFill, Time.deltaTime * chipLerpSpeed);
            if (backFill != null) backFill.color = chipDamageColor;
        }
        else
        {
            backCurrent = Mathf.Lerp(backCurrent, targetFill, Time.deltaTime * (chipLerpSpeed * 0.8f));
            if (backFill != null) backFill.color = chipHealColor;
        }

        // Применяем fillAmount
        if (frontFill != null) frontFill.fillAmount = frontCurrent;
        if (backFill != null) backFill.fillAmount = backCurrent;

        // ПЛАВНАЯ смена цвета: вычисляем цвет по актуальному frontCurrent и плавно интерполируем
        if (colorOverHealth != null && frontFill != null)
        {
            Color targetColor = colorOverHealth.Evaluate(frontCurrent);
            currentFrontColor = Color.Lerp(currentFrontColor, targetColor, Time.deltaTime * colorLerpSpeed);
            frontFill.color = currentFrontColor;
        }

        // Обновляем текст (если есть)
        if (hpText != null && playerHealth != null)
        {
            hpText.text = $"{Mathf.CeilToInt(playerHealth.CurrentHealth)}/{Mathf.CeilToInt(playerHealth.MaxHealth)}";
        }

        // Небольшой пульс при низком HP
        UpdatePulse(frontCurrent);

        // Отладка (временно)
        // Debug.Log($"Update: frontCurrent={frontCurrent:F3}, backCurrent={backCurrent:F3}, targetFill={targetFill:F3}");
    }

    private void HandleHealthChanged(float current, float max)
    {
        float newTarget = Mathf.Clamp01(current / max);
        // Debug.Log($"HandleHealthChanged: targetFill from {targetFill:F3} to {newTarget:F3}");
        targetFill = newTarget;
    }

    private void UpdatePulse(float fill)
    {
        bool low = fill <= lowHpThreshold;
        if (!low)
        {
            transform.localScale = Vector3.one;
            return;
        }

        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = new Vector3(s, s, 1f);
    }
}