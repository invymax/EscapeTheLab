using UnityEngine;

public class AttractHook : MonoBehaviour
{
    public Camera cam;                   // Kamera, is kurios vykdomas raycast
    public Transform player;            // Zaidejo pozicija (pritraukimo taskas)
    public float maxAttractDistance = 10f; // Maksimalus atstumas iki objekto
    public float pullForce = 10f;          // Jega, kuria traukiamas objektas
    public KeyCode attractKey = KeyCode.Mouse1; // Mygtukas, kuri laikant traukiama
    public string attractableTag = "Interactable"; // Tik objektai su siuo tag'u traukiami
    public LineRenderer lr;                 // Linija tarp zaidejo ir objekto

    private Rigidbody targetRb;            // Objektas, kuris bus traukiamas

    private void Update()
    {
        // Kai paspaudziamas mygtukas
        if (Input.GetKeyDown(attractKey))
        {
            // Jei niekas dar netraukiamas
            if (targetRb == null)
            {
                RaycastHit hit;
                // Tikrinam ar spindulys pataike i traukiama objekta
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxAttractDistance))
                {
                    if (hit.collider.CompareTag(attractableTag))
                    {
                        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            targetRb = rb; // Nustatome objekta traukimui
                        }
                    }
                }
            }
        }

        // Kai atleidziamas mygtukas - nutraukiam traukima
        if (Input.GetKeyUp(attractKey))
        {
            targetRb = null;
            if (lr != null)
                lr.positionCount = 0; // Paslepiam linija
        }

        // Nubraiziom spinduli per scena (debug)
        Debug.DrawRay(cam.transform.position, cam.transform.forward * maxAttractDistance, Color.red);
    }

    private void FixedUpdate()
    {
        // Jei objektas nustatytas - traukiam ji link zaidejo
        if (targetRb != null)
        {
            Vector3 direction = (player.position - targetRb.position).normalized;
            targetRb.AddForce(direction * pullForce, ForceMode.Acceleration);

            // Atvaizduojam linija tarp zaidejo ir objekto
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, player.position);
                lr.SetPosition(1, targetRb.position);
            }
        }
    }
}
