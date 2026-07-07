using UnityEngine;
using System;

public class DeliveryZone : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tempo minimo (in secondi) in cui il drone deve rimanere atterrato nella zona per completare la consegna.")]
    [SerializeField] private float requiredLandingDuration = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Indicatore visivo opzionale che mostra se la zona di consegna è attiva.")]
    [SerializeField] private GameObject zoneIndicator;

    private float currentLandingTime = 0f;
    private bool isDroneLanded = false;
    private DroneMovement currentDrone = null;
    private Rigidbody droneRigidbody = null;

    // Evento notificato quando il drone atterra con successo con un pacco
    public event Action<DroneMovement> OnDroneLandedWithPackage;

    public bool IsActiveZone { get; private set; } = false;

    public void SetZoneActive(bool active)
    {
        IsActiveZone = active;
        if (zoneIndicator != null)
        {
            zoneIndicator.SetActive(active);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActiveZone) return;

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
        if (!IsActiveZone || currentDrone == null) return;

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
                    DeliverPackage(currentDrone);
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

    private void DeliverPackage(DroneMovement drone)
    {
        ResetLandingState();
        OnDroneLandedWithPackage?.Invoke(drone);
    }
}
