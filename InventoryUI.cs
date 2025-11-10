using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public PlayerMovement playerMovement;
    public PlayerCamera playerCamera;

    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject detailPanel;

    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text detailDesc;
    public Button useButton;
    public Button cancelButton;

    public KeyCode toggleKey = KeyCode.I;

    InventorySlotUI[] slotUIs;
    int selectedIndex = -1;

    PlayerHealth playerHealth;
    PlayerEffects playerEffects;

    void Start()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (inventory)
        {
            playerHealth = inventory.GetComponent<PlayerHealth>();
            playerEffects = inventory.GetComponent<PlayerEffects>();
        }

        if (!playerMovement) playerMovement = FindObjectOfType<PlayerMovement>();
        if (!playerCamera) playerCamera = FindObjectOfType<PlayerCamera>();

        slotUIs = slotsParent.GetComponentsInChildren<InventorySlotUI>(true);
        for (int i = 0; i < slotUIs.Length; i++)
        {
            int idx = i;
            slotUIs[i].SetIndex(idx);
            slotUIs[i].onSingleClick = OnSlotSingleClick;
            slotUIs[i].onDoubleClick = OnSlotDoubleClick;
        }

        if (useButton) useButton.onClick.AddListener(OnUsePressed);
        if (cancelButton) cancelButton.onClick.AddListener(CloseDetail);

        if (inventory) inventory.OnInventoryChanged += RefreshSlots;

        if (inventoryPanel) inventoryPanel.SetActive(false);
        if (detailPanel) detailPanel.SetActive(false);

        RefreshSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleInventory();
    }

    void ToggleInventory()
    {
        if (!inventoryPanel) return;
        bool open = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(open);

        if (playerMovement) playerMovement.inputEnabled = !open;
        if (playerCamera)
        {
            playerCamera.EnableInput(!open, true);
            playerCamera.SetCursorLock(!open);
        }
        else
        {
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        if (!open) CloseDetail();
    }

    void RefreshSlots()
    {
        if (!inventory || slotUIs == null) return;

        for (int i = 0; i < slotUIs.Length; i++)
        {
            var slot = (i < inventory.slots.Length) ? inventory.slots[i] : default;
            Sprite icon = slot.item ? slot.item.icon : null;
            string name = slot.item ? slot.item.itemName : "";
            bool selected = (i == selectedIndex);
            slotUIs[i].Setup(icon, name, selected);
        }

        if (selectedIndex >= 0 && inventory.slots[selectedIndex].IsEmpty)
            CloseDetail();
        else if (selectedIndex >= 0)
            UpdateDetailUseState();
    }

    void OnSlotSingleClick(int idx)
    {
        selectedIndex = idx;
        ShowDetail(idx);
    }

    void OnSlotDoubleClick(int idx)
    {
        var slot = inventory.slots[idx];
        if (slot.IsEmpty) return;

        if (!CanUseItem(slot.item))
        {
            // —ообщение игроку (пока логом)
            Debug.Log("Ќельз€ использовать: условие не выполнено (например, полное HP).");
            return;
        }

        if (inventory.UseItem(idx))
        {
            selectedIndex = -1;
            if (detailPanel) detailPanel.SetActive(false);
            RefreshSlots();
        }
    }

    void ShowDetail(int idx)
    {
        if (!detailPanel) return;
        if (idx < 0 || idx >= inventory.slots.Length)
        {
            CloseDetail();
            return;
        }

        var slot = inventory.slots[idx];
        if (slot.IsEmpty)
        {
            CloseDetail();
            return;
        }

        detailPanel.SetActive(true);

        if (detailIcon) detailIcon.sprite = slot.item.icon;
        if (detailName) detailName.text = slot.item.itemName;
        if (detailDesc) detailDesc.text = ItemDescriptionBuilder.Build(slot.item);

        UpdateDetailUseState();
        RefreshSlots();
    }

    void UpdateDetailUseState()
    {
        if (useButton == null) return;
        if (selectedIndex < 0 || selectedIndex >= inventory.slots.Length)
        {
            useButton.interactable = false;
            return;
        }
        var slot = inventory.slots[selectedIndex];
        useButton.interactable = !slot.IsEmpty && CanUseItem(slot.item);
    }

    bool CanUseItem(ItemDefinition item)
    {
        if (!item) return false;

        switch (item.itemType)
        {
            case ItemType.Heal:
                // Ќельз€ если здоровье уже полное
                if (playerHealth != null && playerHealth.IsFull) return false;
                return true;
            case ItemType.InvisibilityBracelet:
                // ћожно, если сейчас не в невидимости (опционально)
                if (playerEffects != null)
                {
                    // нет публичного флага Ч допускаем всегда
                    return true;
                }
                return true;
            case ItemType.DecoyBall:
            case ItemType.StoryFlashDrive:
                return true;
            default:
                return true;
        }
    }

    void OnUsePressed()
    {
        if (selectedIndex < 0) return;
        var slot = inventory.slots[selectedIndex];
        if (slot.IsEmpty) { CloseDetail(); return; }

        if (!CanUseItem(slot.item))
        {
            Debug.Log("Use: предмет нельз€ применить (например, полное HP).");
            UpdateDetailUseState();
            return;
        }

        if (inventory.UseItem(selectedIndex))
        {
            selectedIndex = -1;
            if (detailPanel) detailPanel.SetActive(false);
            RefreshSlots();
        }
        else
        {
            // не применилс€ (например, проверка внутри PlayerEffects)
            UpdateDetailUseState();
        }
    }

    void CloseDetail()
    {
        selectedIndex = -1;
        if (detailPanel) detailPanel.SetActive(false);
        RefreshSlots();
    }

    void OnDestroy()
    {
        if (inventory) inventory.OnInventoryChanged -= RefreshSlots;
    }
}