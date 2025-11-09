using UnityEngine;

public class SwingHook : MonoBehaviour
{
    public LineRenderer lr;                   // Linija tarp ginklo ir kablio
    public Transform gunTip, cam, player;     // Ginklo galas, kamera, zaidejas
    public LayerMask whatIsGrappleable;       // Objektai, prie kuriu galima prisikabinti

    private float maxSwingDistance = 10f;     // Maksimalus atstumas iki kablio tasko

    private Vector3 swingPoint;               // Kabliavimo taskas
    private SpringJoint joint;                // Spyruokline jungtis

    public KeyCode swingKey = KeyCode.Mouse0; // Mygtukas, kuriuo aktyvuojamas suolis

    private void Awake()
    {
        lr.positionCount = 0; // Pradzioje linija nerodoma
    }

    private void Update()
    {
        // Pradeti suoli
        if (Input.GetKeyDown(swingKey)) StartSwing();

        // Nutraukti suoli
        if (Input.GetKeyUp(swingKey)) StopSwing();
    }

    private void LateUpdate()
    {
        DrawRope(); // Nupiesti virve tarp zaidejo ir kablio
    }

    private void StartSwing()
    {
        RaycastHit hit;

        // Tikriname, ar priesais yra objektas, prie kurio galima prisikabinti
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
            swingPoint = hit.point;

            // Pridedame spyruokline jungti tarp zaidejo ir kablio tasko
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // Nustatome spyruokles ilgi
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            // Spyruokles parametrai
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2; // Aktyvuojame linijos piesima
        }
    }

    public void StopSwing()
    {
        lr.positionCount = 0; // Paslepiam virve
        Destroy(joint);       // Pasalinam jungti
    }

    private Vector3 currentGrapplePosition;

    private void DrawRope()
    {
        if (!joint) return;

        // Svelniai animuojame linijos gala link kablio tasko
        currentGrapplePosition =
            Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);         // Pradzia nuo ginklo
        lr.SetPosition(1, currentGrapplePosition);  // Galas link kablio
    }
}
