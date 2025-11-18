using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "EscapeTheLab/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Effect parameters")]
    public int healAmount = 25;
    public float invisDuration = 7f;
    public AudioClip useSound;

    [Header("Decoy (optional)")]
    public GameObject decoyPrefab;
    public float decoyThrowForce = 8f;
}