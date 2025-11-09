using UnityEngine;

public class OutlineHighlighter : MonoBehaviour
{
    public Camera cam;
    public float maxDistance = 15f;

    public string interactableTag = "Interactable";
    public string hookLayerName = "Hook"; 

    private Outline currentOutline;

    void Update()
    {
        // Sukuriamas spindulys is kameros į prieki
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        // Patikriname, ar spindulys pataiko i ka nors per maxDistance
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            // Ar objektas turi tinkamą tag arba yra tinkamame sluoksnyje
            bool isInteractable = hit.collider.CompareTag(interactableTag);
            bool isHook = hitObj.layer == LayerMask.NameToLayer(hookLayerName);

            if (isInteractable || isHook)
            {
                // Paimame Outline komponentą (jeigu nera – pridedame)
                Outline outline = hitObj.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = hitObj.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineColor = isHook ? Color.green : Color.cyan; // skirtinga spalva kabliui
                    outline.OutlineWidth = 6f;
                }

                // Ijungiame kontura naujam objektui, isjungiame sena
                if (currentOutline != outline)
                {
                    if (currentOutline != null)
                        currentOutline.enabled = false;

                    outline.enabled = true;
                    currentOutline = outline;
                }

                return; // sustabdome funkcija, jei radome tinkama objekta
            }
        }

        // Jei nieko nepasirinkta – isjungiame kontura
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
