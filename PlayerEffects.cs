using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Inventory))]
public class PlayerEffects : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Renderer[] bodyRenderers;
    public Material invisibleMaterial;        // если null — просто выключаем рендеры
    public AudioSource audioSource;

    [Header("Invisibility Settings")]
    [Tooltip("Имя слоя для невидимости. Должен существовать в Tags & Layers.")]
    public string invisibleLayerName = "PlayerInvisible";
    [Tooltip("Имя тега для невидимости (опционально). Оставь пустым если не нужно.")]
    public string invisibleTagName = "";              // например "InvisiblePlayer"
    [Tooltip("Менять layer и tag также у детей (коллайдеров, рендеров)?")]
    public bool changeChildrenLayers = true;
    public bool changeChildrenTags = false;

    [Tooltip("Длительность невидимости по умолчанию (если предмет не задаёт).")]
    public float defaultInvisibilityDuration = 7f;

    [Header("Flash Drive")]
    public float memoryDuration = 5f;

    int originalLayer;
    string originalTag;
    Material[] originalMaterials;
    bool visibilityMaterialsStored;

    public bool IsInvisible { get; private set; }

    void Awake()
    {
        if (!playerHealth) playerHealth = GetComponent<PlayerHealth>();
        originalLayer = gameObject.layer;
        originalTag = gameObject.tag;

        if (bodyRenderers != null && bodyRenderers.Length > 0)
        {
            originalMaterials = new Material[bodyRenderers.Length];
            for (int i = 0; i < bodyRenderers.Length; i++)
                originalMaterials[i] = bodyRenderers[i].material;
            visibilityMaterialsStored = true;
        }
    }

    public bool ApplyItem(ItemDefinition def)
    {
        if (!def) return false;

        switch (def.itemType)
        {
            case ItemType.Heal:
                return ApplyHeal(def.healAmount, def.useSound);

            case ItemType.InvisibilityBracelet:
                return StartInvisibility(def.invisDuration > 0 ? def.invisDuration : defaultInvisibilityDuration,
                                         def.useSound);

            case ItemType.DecoyBall:
                return ThrowDecoy(def, def.useSound);

            case ItemType.StoryFlashDrive:
                return TriggerFlashDrive(def.useSound);

            default:
                Debug.LogWarning("Unknown item type: " + def.itemType);
                return false;
        }
    }

    bool ApplyHeal(int amount, AudioClip snd)
    {
        if (!playerHealth) return false;
        if (playerHealth.IsFull)
        {
            Debug.Log("Heal ignored: HP is already full.");
            return false;
        }
        playerHealth.Heal(amount);
        PlaySound(snd);
        return true;
    }

    bool StartInvisibility(float duration, AudioClip snd)
    {
        if (IsInvisible) return false; // уже невидим
        PlaySound(snd);
        StartCoroutine(InvisibilityRoutine(duration));
        return true;
    }

    IEnumerator InvisibilityRoutine(float duration)
    {
        IsInvisible = true;

        // Сменить визуал
        if (invisibleMaterial != null && visibilityMaterialsStored)
        {
            for (int i = 0; i < bodyRenderers.Length; i++)
                bodyRenderers[i].material = invisibleMaterial;
        }
        else if (bodyRenderers != null)
        {
            foreach (var r in bodyRenderers)
                r.enabled = false;
        }

        // Смена layer
        int newLayer = -1;
        if (!string.IsNullOrEmpty(invisibleLayerName))
        {
            newLayer = LayerMask.NameToLayer(invisibleLayerName);
            if (newLayer < 0)
                Debug.LogWarning("Invisibility: layer '" + invisibleLayerName + "' not found. No layer change.");
            else
            {
                gameObject.layer = newLayer;
                if (changeChildrenLayers)
                    ApplyLayerToChildren(transform, newLayer);
            }
        }

        // Смена tag (опционально)
        if (!string.IsNullOrEmpty(invisibleTagName))
        {
            originalTag = gameObject.tag;
            gameObject.tag = invisibleTagName;
            if (changeChildrenTags)
                ApplyTagToChildren(transform, invisibleTagName);
        }

        float t = duration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        // Восстановить визуал
        if (invisibleMaterial != null && visibilityMaterialsStored)
        {
            for (int i = 0; i < bodyRenderers.Length; i++)
                bodyRenderers[i].material = originalMaterials[i];
        }
        else if (bodyRenderers != null)
        {
            foreach (var r in bodyRenderers)
                r.enabled = true;
        }

        // Восстановить layer
        if (newLayer >= 0)
        {
            gameObject.layer = originalLayer;
            if (changeChildrenLayers)
                ApplyLayerToChildren(transform, originalLayer);
        }

        // Восстановить tag
        if (!string.IsNullOrEmpty(invisibleTagName))
        {
            gameObject.tag = originalTag;
            if (changeChildrenTags)
                ApplyTagToChildren(transform, originalTag);
        }

        IsInvisible = false;
    }

    bool ThrowDecoy(ItemDefinition def, AudioClip snd)
    {
        if (!def.decoyPrefab) return false;
        PlaySound(snd);
        Vector3 spawnPos = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
        var go = Instantiate(def.decoyPrefab, spawnPos, Quaternion.identity);
        var rb = go.GetComponent<Rigidbody>();
        if (rb)
            rb.AddForce((transform.forward + Vector3.up * 0.25f) * def.decoyThrowForce, ForceMode.VelocityChange);
        return true;
    }

    bool TriggerFlashDrive(AudioClip snd)
    {
        PlaySound(snd);
        var movement = GetComponent<PlayerMovement>();
        if (movement) movement.inputEnabled = false;

        var memoryUI = FindObjectOfType<MemoryUI>();
        if (memoryUI)
            memoryUI.ShowMemory("Флешка: ты вспоминаешь конфликт с TITAS...");

        Invoke(nameof(RestoreControl), memoryDuration);
        return true;
    }

    void RestoreControl()
    {
        var movement = GetComponent<PlayerMovement>();
        if (movement) movement.inputEnabled = true;
        var memoryUI = FindObjectOfType<MemoryUI>();
        if (memoryUI) memoryUI.HideMemory();
    }

    void PlaySound(AudioClip snd)
    {
        if (snd && audioSource) audioSource.PlayOneShot(snd);
    }

    void ApplyLayerToChildren(Transform root, int layer)
    {
        foreach (Transform c in root)
        {
            c.gameObject.layer = layer;
            ApplyLayerToChildren(c, layer);
        }
    }

    void ApplyTagToChildren(Transform root, string tagName)
    {
        foreach (Transform c in root)
        {
            c.gameObject.tag = tagName;
            ApplyTagToChildren(c, tagName);
        }
    }
}