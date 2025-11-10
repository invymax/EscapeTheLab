using UnityEngine;

public class InventoryClickTester : MonoBehaviour
{
    public InventoryUI ui;

    void Start()
    {
        if (!ui) ui = FindObjectOfType<InventoryUI>();
    }

    void Update()
    {
        // Нажми клавиши 0..7 чтобы принудительно вызвать OnSlotSingleClick
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                Debug.Log($"[Tester] Simulate click slot {i}");
                typeof(InventoryUI)
                    .GetMethod("OnSlotSingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(ui, new object[] { i });
            }
        }
    }
}