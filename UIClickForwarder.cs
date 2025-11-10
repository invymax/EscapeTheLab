using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIClickForwarder : MonoBehaviour
{
    [Header("Refs")]
    public InventoryUI inventoryUI;            // перетащи объект с InventoryUI
    public RectTransform slotsRoot;            // SlotsContainer
    public EventSystem eventSystem;            // EventSystem (или оставь пустым)

    [Header("Config")]
    public float doubleClickThreshold = 0.3f;
    public bool rightClickAsSingle = true;
    public bool handleDetailButtons = true;    // вручную обрабатывать Use/Cancel

    int lastSlot = -1;
    float lastClickTime = -999f;

    void Awake()
    {
        if (!inventoryUI) inventoryUI = FindObjectOfType<InventoryUI>();
        if (!eventSystem) eventSystem = EventSystem.current;
        if (!slotsRoot && inventoryUI) slotsRoot = inventoryUI.slotsParent as RectTransform;
    }

    void Update()
    {
        if (!inventoryUI || !inventoryUI.inventoryPanel || !inventoryUI.inventoryPanel.activeInHierarchy)
            return;

        if (Input.GetMouseButtonDown(0)) HandleClick(false);
        if (Input.GetMouseButtonDown(1)) HandleClick(true);
    }

    void HandleClick(bool isRightButton)
    {
        if (eventSystem == null) return;

        PointerEventData ped = new PointerEventData(eventSystem) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(ped, results);
        if (results.Count == 0) return;

        // 1. Попытаться найти слот
        foreach (var r in results)
        {
            var slot = r.gameObject.GetComponentInParent<InventorySlotUI>();
            if (slot != null)
            {
                if (slotsRoot != null && !slot.transform.IsChildOf(slotsRoot)) continue;

                int idx = slot.slotIndex;
                float now = Time.unscaledTime;

                if (isRightButton)
                {
                    if (rightClickAsSingle)
                        slot.onSingleClick?.Invoke(idx);
                    return;
                }

                bool isDouble = (idx == lastSlot) && (now - lastClickTime <= doubleClickThreshold);
                if (isDouble)
                {
                    if (slot.onDoubleClick != null) slot.onDoubleClick(idx);
                    else slot.onSingleClick?.Invoke(idx);

                    lastSlot = -1;
                    lastClickTime = -999f;
                }
                else
                {
                    slot.onSingleClick?.Invoke(idx);
                    lastSlot = idx;
                    lastClickTime = now;
                }
                return; // слот обработан
            }
        }

        // 2. Если не слот, но клик внутри панельки детализации — обработать кнопки вручную
        if (handleDetailButtons && inventoryUI.detailPanel && inventoryUI.detailPanel.activeInHierarchy)
        {
            foreach (var r in results)
            {
                var btn = r.gameObject.GetComponentInParent<Button>();
                if (btn == null) continue;

                if (btn == inventoryUI.useButton)
                {
                    inventoryUI.SendMessage("OnUsePressed", SendMessageOptions.DontRequireReceiver);
                    return;
                }
                if (btn == inventoryUI.cancelButton)
                {
                    inventoryUI.SendMessage("CloseDetail", SendMessageOptions.DontRequireReceiver);
                    return;
                }
            }
        }
    }
}