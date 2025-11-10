using UnityEngine;

public class OutlineHighlighter : MonoBehaviour
{
    public Camera cam;
    public float maxDistance = 15f;

    public string interactableTag = "Interactable";
    public string hookLayerName = "Hook";

    private Outline currentOutline;
    private GameObject currentTarget;

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            GameObject hitObj = hit.collider.gameObject;
            bool isInteractable = hit.collider.CompareTag(interactableTag);
            bool isHook = hitObj.layer == LayerMask.NameToLayer(hookLayerName);

            if (isInteractable || isHook)
            {
                // Если целевой объект изменился — переключаем outline
                if (hitObj != currentTarget)
                {
                    // Выключаем предыдущий
                    if (currentOutline != null)
                    {
                        currentOutline.enabled = false;
                        currentOutline = null;
                    }

                    currentTarget = hitObj;
                    // Пытаемся получить существующий Outline (не создаём всегда)
                    Outline outline = hitObj.GetComponent<Outline>();
                    if (outline == null)
                    {
                        outline = hitObj.AddComponent<Outline>(); // создаём только один раз
                        outline.OutlineMode = Outline.Mode.OutlineAll;
                        outline.OutlineColor = isHook ? Color.green : Color.cyan;
                        outline.OutlineWidth = 6f;
                    }

                    currentOutline = outline;
                    currentOutline.enabled = true;
                }

                // не делаем return — но это ок, мы управляли переключением выше
                return;
            }
        }

        // Если ничего не найдено — выключаем прошлый
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
            currentTarget = null;
        }
    }
}