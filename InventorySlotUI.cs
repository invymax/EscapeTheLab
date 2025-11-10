using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TMP_Text nameText;
    public GameObject highlight;

    [HideInInspector] public int slotIndex = -1;

    [Header("Click events")]
    public Action<int> onSingleClick;
    public Action<int> onDoubleClick;

    [Header("Internal click handler")]
    [Tooltip("Если true — встроенный OnPointerClick будет игнорироваться, клики обрабатывает UIClickForwarder.")]
    public bool ignoreUnityPointerEvents = true;

    [Header("Double click (если не используешь forwarder)")]
    public float doubleClickThreshold = 0.3f;
    int clickCount;
    float lastClickStart;

    public void SetIndex(int i) => slotIndex = i;

    public void Setup(Sprite sprite, string itemName, bool selected)
    {
        if (icon)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }
        if (nameText)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                nameText.text = itemName;
                nameText.gameObject.SetActive(true);
            }
            else
            {
                nameText.text = "";
                nameText.gameObject.SetActive(false);
            }
        }
        if (highlight) highlight.SetActive(selected);
    }

    // Этот обработчик можно оставить на случай, если позже починишь InputModule и захочешь отключить forwarder.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ignoreUnityPointerEvents) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        float now = Time.unscaledTime;

        if (clickCount == 0)
        {
            clickCount = 1;
            lastClickStart = now;
            Invoke(nameof(ResolveClick), doubleClickThreshold);
        }
        else
        {
            if (now - lastClickStart <= doubleClickThreshold)
            {
                clickCount = 2;
                CancelInvoke(nameof(ResolveClick));
                ResolveClick();
            }
        }
    }

    void ResolveClick()
    {
        if (clickCount >= 2) onDoubleClick?.Invoke(slotIndex);
        else if (clickCount == 1) onSingleClick?.Invoke(slotIndex);

        clickCount = 0;
    }
}