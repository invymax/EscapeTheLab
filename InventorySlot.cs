using System;

[Serializable]
public struct InventorySlot
{
    public ItemDefinition item;

    public bool IsEmpty => item == null;

    public void Clear()
    {
        item = null;
    }
}