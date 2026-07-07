using UnityEngine;
using System;

public class PackageStation : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tempo minimo (in secondi) in cui il drone deve rimanere atterrato sulla stazione per ritirare il pacco.")]
    [SerializeField] private float requiredLandingDuration = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Prefab del modello visivo del pacco da mostrare sulla stazione di ritiro.")]
    [SerializeField] private GameObject packageVisualPrefab;
    [Tooltip("Offset locale per posizionare il pacco visivo sulla stazione.")]
    [SerializeField] private Vector3 packageOffset = new Vector3(0f, 1f, 0f);

    private float currentLandingTime = 0f;
    private bool isDroneLanded = false;
    private DroneMovement currentDrone = null;
    private Rigidbody droneRigidbody = null;
    private GameObject currentPackageVisualInstance = null;

    // Evento notificato al ritiro del pacco
    public event Action<DroneMovement> OnPackagePickedUp;

    public bool HasPackage { get; private set; } = false;

    public void SpawnPackage()
    {
        if (HasPackage) return;

        HasPackage = true;
        if (packageVisualPrefab != null && currentPackageVisualInstance == null)
        {
            currentPackageVisualInstance = Instantiate(packageVisualPrefab, transform.position + packageOffset, Quaternion.identity, transform);
        }
        else if (packageVisualPrefab == null)
        {
            Debug.LogWarning("PackageStation: packageVisualPrefab non assegnato, il pacco non sarà visibile sulla stazione.");
        }
    }

    public void RemovePackageVisual()
    {
        if (currentPackageVisualInstance != null)
        {
            Destroy(currentPackageVisualInstance);
            currentPackageVisualInstance = null;
        }
        HasPackage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasPackage) return;

        DroneMovement drone = other.GetComponentInParent<DroneMovement>();
        if (drone != null)
        {
            currentDrone = drone;
            droneRigidbody = drone.GetComponent<Rigidbody>();
            isDroneLanded = false;
            currentLandingTime = 0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!HasPackage || currentDrone == null) return;

        // Il drone è considerato atterrato se è grounded e la sua velocità è minima
        bool isStopped = droneRigidbody == null || droneRigidbody.linearVelocity.magnitude < 0.5f;

        if (currentDrone.IsGrounded && isStopped)
        {
            if (!isDroneLanded)
            {
                isDroneLanded = true;
                currentLandingTime = 0f;
            }
            else
            {
                currentLandingTime += Time.deltaTime;
                if (currentLandingTime >= requiredLandingDuration)
                {
                    PickupPackage(currentDrone);
                }
            }
        }
        else
        {
            isDroneLanded = false;
            currentLandingTime = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DroneMovement drone = other.GetComponentInParent<DroneMovement>();
        if (drone != null && drone == currentDrone)
        {
            ResetLandingState();
        }
    }

    private void ResetLandingState()
    {
        currentDrone = null;
        droneRigidbody = null;
        isDroneLanded = false;
        currentLandingTime = 0f;
    }

    private void PickupPackage(DroneMovement drone)
    {
        RemovePackageVisual();
        ResetLandingState();
        OnPackagePickedUp?.Invoke(drone);
    }
}
