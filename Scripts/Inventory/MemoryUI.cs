using UnityEngine;
using TMPro;

public class MemoryUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text memoryText;

    void Start()
    {
        if (panel) panel.SetActive(false);
    }

    public void ShowMemory(string text)
    {
        if (memoryText) memoryText.text = text;
        if (panel) panel.SetActive(true);
        // ћожно запустить анимацию / затемнение / звук
    }

    public void HideMemory()
    {
        if (panel) panel.SetActive(false);
    }
}