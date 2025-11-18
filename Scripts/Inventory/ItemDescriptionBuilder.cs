using UnityEngine;
using System.Text;

public static class ItemDescriptionBuilder
{
    public static string Build(ItemDefinition def)
    {
        if (!def) return "";
        if (!string.IsNullOrWhiteSpace(def.description))
            return def.description;

        // Автогенерация, если description пустой
        var sb = new StringBuilder();
        switch (def.itemType)
        {
            case ItemType.Heal:
                sb.Append("Atkuria sveikatą: ").Append(def.healAmount).Append(" HP!");
                break;
            case ItemType.InvisibilityBracelet:
                sb.Append("Būk nematomas ").Append(Mathf.RoundToInt(def.invisDuration)).Append(" sekundes!");
                break;
            case ItemType.DecoyBall:
                sb.Append("Отвлекает патрульных. Бросок вперёд.");
                break;
            case ItemType.StoryFlashDrive:
                sb.Append("Запускает воспоминание.");
                break;
            default:
                sb.Append("Предмет.");
                break;
        }
        return sb.ToString();
    }
}