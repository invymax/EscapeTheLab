using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    public ItemDefinition itemDefinition;
    public AudioClip pickupSound;

    private void Awake()
    {
        // Collider должен быть trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var inv = other.GetComponentInParent<Inventory>();
        if (inv == null)
        {
            Debug.Log("инвентарь нет");

            return;

        }
        if (itemDefinition == null) return;

        bool added = inv.TryAdd(itemDefinition);
        if (added)
        {
            if (pickupSound)
            {
                var src = other.GetComponent<AudioSource>();
                if (src) src.PlayOneShot(pickupSound);
            }
            Destroy(gameObject);
        }
        else
        {
            // Можно показать всплывающий текст "Инвентарь полон"
            Debug.Log("Инвентарь полон!");
        }
    }
}