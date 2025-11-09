using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowingGrenade : MonoBehaviour
{
    public GameObject grenadePrefab;        // Granatos prefabas
    private KeyCode throwKey = KeyCode.G;   // Mestas mygtukas
    public Transform throwPosition;         // Pozicija is kurios metama
    public Vector3 throwDirection = new Vector3(0, 1, 0); // Papildomas krypties poslinkis

    private float throwForce = 10f;         // Pradine jega
    private float maxForce = 20f;           // Maksimali jega

    public LineRenderer lineRenderer;       // Linija trajektorijai rodyti

    private bool isCharging = false;        // Ar laikomas mygtukas
    private float chargeTime = 0f;          // Kiek laiko laikomas
    public Camera mainCamera;               // Pagrindine kamera

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Pradedame krauti kai paspaustas G
        if (Input.GetKeyDown(throwKey))
        {
            StartThrowing();
        }

        // Kraunam jega kol laikomas mygtukas
        if (isCharging)
        {
            ChargeThrow();
        }

        // Kai paleidziamas mygtukas - metame
        if (Input.GetKeyUp(throwKey))
        {
            ReleaseThrow();
        }
    }

    // Paruosiam metima
    private void StartThrowing()
    {
        isCharging = true;
        chargeTime = 0f;

        lineRenderer.enabled = true; // Ijungiame trajektorijos linija
    }

    // Krovimo metu apskaiciuojama trajektorija ir rodomas kelias
    void ChargeThrow()
    {
        chargeTime += Time.deltaTime;

        Vector3 grenadeVelocity = (mainCamera.transform.forward + throwDirection).normalized
                                  * Mathf.Min(chargeTime * throwForce, maxForce);

        ShowTrajectory(throwPosition.position, grenadeVelocity);
    }

    // Atleidus mygtuka - vykdomas metimas
    void ReleaseThrow()
    {
        ThrowGrenade(Mathf.Min(chargeTime * throwForce, maxForce));
        isCharging = false;

        lineRenderer.enabled = false; // Paslepiame trajektorijos linija
    }

    // Sukuriame granata ir pritaikome jai jega
    void ThrowGrenade(float force)
    {
        Vector3 spawnPosition = throwPosition.position + mainCamera.transform.forward;

        GameObject grenade = Instantiate(grenadePrefab, spawnPosition, mainCamera.transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        Vector3 finalThrowDirection = (mainCamera.transform.forward + throwDirection).normalized;
        rb.AddForce(finalThrowDirection * force, ForceMode.VelocityChange);
    }

    // Piesiame trajektorija su LineRenderer
    void ShowTrajectory(Vector3 origin, Vector3 speed)
    {
        int pointCount = 30;
        Vector3[] points = new Vector3[pointCount];
        lineRenderer.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float time = i * 0.1f;
            points[i] = origin + speed * time + 0.5f * Physics.gravity * time * time;
        }

        lineRenderer.SetPositions(points);
    }
}
