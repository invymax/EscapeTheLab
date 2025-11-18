using UnityEngine;

public interface IInteractable
{
    void Interact(); // Kiekvienas objektas, su kuriuo galima bendrauti, turi si metoda
}

public class InteractHook : MonoBehaviour
{
    public Camera cam;                     // Kamera, is kurios vykdomas raycast
    public Transform gunPoint;            // Ginklo taskas (kur prasideda linija)
    public float maxInteractDistance = 15f; // Maksimalus atstumas iki objekto
    public KeyCode interactKey = KeyCode.Mouse1; // Mygtukas, kuris naudojamas bendrauti
    public string interactableTag = "Interactable"; // Tik su siuo tag'u bendraujama
    public LineRenderer lr;               // Linija vizualiam efektui rodyti
    private Outline currentOutline;       // Siuo metu aktyvus konturas (jei yra)

    private bool isInteracting = false;   // Ar vyksta sasaja
    private Vector3 hitPoint;             // Kur pataike raycast

    private void Update()
    {
        // Kai paspaudziamas mygtukas
        if (Input.GetKeyDown(interactKey))
        {
            RaycastHit hit;
            // Siunciame spinduli is kameros pirmyn
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxInteractDistance))
            {
                // Jei objektas turi tinkama taga
                if (hit.collider.CompareTag(interactableTag))
                {
                    // Vykdome objekto interakcija, jei jis tai palaiko
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }

                    // Nustatome sasajos taska ir ijungiame linija
                    hitPoint = hit.point;
                    isInteracting = true;

                    if (lr != null)
                    {
                        lr.positionCount = 2;
                        lr.SetPosition(0, gunPoint.position);
                        lr.SetPosition(1, hitPoint);
                    }
                }
            }
        }

        // Kol laikomas mygtukas – atnaujiname linijos taskus
        if (Input.GetKey(interactKey) && isInteracting)
        {
            if (lr != null && lr.positionCount == 2)
            {
                lr.SetPosition(0, gunPoint.position);
                lr.SetPosition(1, hitPoint);
            }
        }

        // Paleidus mygtuka – isjungiame interakcija ir linija
        if (Input.GetKeyUp(interactKey))
        {
            isInteracting = false;
            if (lr != null)
            {
                lr.positionCount = 0;
            }
        }

        // Jei nebeinteraktuojama – isjungiame kontura
        if (!Input.GetKey(interactKey) && currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }

        // Nubraiziom spinduli scenoje (debug)
        Debug.DrawRay(cam.transform.position, cam.transform.forward * maxInteractDistance, Color.blue);
    }
}
