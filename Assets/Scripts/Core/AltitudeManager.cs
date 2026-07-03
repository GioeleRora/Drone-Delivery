using UnityEngine;
using System;

[RequireComponent(typeof(DroneMovement))]
public class AltitudeManager : MonoBehaviour
{
    [Header("Altitude Settings")]
    [Tooltip("L'altezza oltre la quale appaiono gli avvisi di pericolo")]
    [SerializeField] private float warningAltitude = 50f;
    [Tooltip("L'altezza massima consentita prima dello spegnimento dei motori")]
    [SerializeField] private float criticalAltitude = 65f;

    private DroneMovement droneMovement;
    private bool areMotorsDisabledByAltitude = false;

    // Eventi utili per la UI (per mostrare a schermo "ATTENZIONE: VENTO FORTE")
    public event Action OnWarningAltitudeReached;
    public event Action OnSafeAltitudeReturned;

    private bool isWarningActive = false;

    private void Awake()
    {
        droneMovement = GetComponent<DroneMovement>();
    }

    private void Update()
    {
        CheckAltitude();
    }

    private void CheckAltitude()
    {
        float currentHeight = transform.position.y;

        // Gestione Spegnimento Motori (Critical Altitude)
        if (currentHeight >= criticalAltitude && !areMotorsDisabledByAltitude)
        {
            areMotorsDisabledByAltitude = true;
            droneMovement.SetMotorsState(false);
            Debug.LogWarning("Altitudine critica raggiunta! Vento troppo forte. Spegnimento motori.");
        }
        else if (currentHeight < criticalAltitude && areMotorsDisabledByAltitude)
        {
            // Nota: Se la batteria è a zero, il BatterySystem spegnerà di nuovo i motori 
            // ma gestiamo solo la nostra responsabilità qui (SOLID)
            areMotorsDisabledByAltitude = false;
            droneMovement.SetMotorsState(true);
            Debug.Log("Rientro in altitudine sicura. Riaccensione motori.");
        }

        // Gestione Avvisi UI (Warning Altitude)
        if (currentHeight >= warningAltitude && currentHeight < criticalAltitude && !isWarningActive)
        {
            isWarningActive = true;
            OnWarningAltitudeReached?.Invoke();
            Debug.Log("Attenzione: Altitudine pericolosa!");
        }
        else if (currentHeight < warningAltitude && isWarningActive)
        {
            isWarningActive = false;
            OnSafeAltitudeReturned?.Invoke();
            Debug.Log("Altitudine sicura.");
        }
    }
}
