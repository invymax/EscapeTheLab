using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int slotCount = 8;
    public InventorySlot[] slots;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        slots = new InventorySlot[slotCount];
    }

    // Добавление — возвращает true если вошло
    public bool TryAdd(ItemDefinition def)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].item = def;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false; // нет свободных слотов
    }

    public bool UseItem(int index)
    {
        if (index < 0 || index >= slots.Length) return false;
        var item = slots[index].item;
        if (item == null) return false;

        // применение эффекта
        var effects = GetComponent<PlayerEffects>();
        if (effects == null)
        {
            Debug.LogWarning("Inventory.UseItem: PlayerEffects отсутствует на объекте игрока.");
            return false;
        }

        bool applied = effects.ApplyItem(item);
        if (applied)
        {
            slots[index].Clear();
            OnInventoryChanged?.Invoke();
        }
        return applied;
    }
}