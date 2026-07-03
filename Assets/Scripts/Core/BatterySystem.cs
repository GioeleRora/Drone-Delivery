using UnityEngine;

[RequireComponent(typeof(DroneMovement))]
public class BatterySystem : MonoBehaviour
{
    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float idleDrainRate = 1f; // Consumo per il solo fatto di stare in hovering
    [SerializeField] private float movementDrainMultiplier = 0.5f; // Consumo aggiuntivo mentre ci si muove

    private DroneMovement droneMovement;
    private float currentBattery;
    private bool isBatteryEmpty = false;

    private void Awake()
    {
        droneMovement = GetComponent<DroneMovement>();
        currentBattery = maxBattery;
    }

    private void Update()
    {
        if (isBatteryEmpty) return;

        DrainBattery();
        CheckBatteryStatus();
    }

    private void DrainBattery()
    {
        // Consumo base (hovering)
        float totalDrain = idleDrainRate;

        // Consumo aggiuntivo in base all'input
        // Più ci muoviamo o saliamo, più consumiamo (per simulare lo sforzo dei motori)
        // L'asse Y (Vertical) e l'altitudine (ascend) aumentano il consumo
        float inputMagnitude = Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"));
        
        // Se stiamo salendo consumiamo ancora di più
        float altitudeEffort = Input.GetKey(KeyCode.Space) ? 1.5f : 0f;

        totalDrain += (inputMagnitude + altitudeEffort) * movementDrainMultiplier;

        // Riduciamo la batteria in base al tempo trascorso (indipendente dal framerate)
        currentBattery -= totalDrain * Time.deltaTime;
        
        // Evitiamo che la batteria scenda sotto zero
        currentBattery = Mathf.Max(currentBattery, 0f);
    }

    private void CheckBatteryStatus()
    {
        if (currentBattery <= 0f && !isBatteryEmpty)
        {
            isBatteryEmpty = true;
            Debug.LogWarning("Batteria esaurita! I motori si spengono.");
            
            // Spegniamo i motori tramite il componente DroneMovement
            droneMovement.SetMotorsState(false);
            
            // TODO: Segnalare al Game Manager che la missione è fallita o in stato critico
        }
    }

    // Metodi pubblici utili per il futuro (es. raccolta PowerUp, UI)
    public float GetCurrentBatteryPercentage()
    {
        return currentBattery / maxBattery;
    }

    public void Recharge(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, maxBattery);
        if (currentBattery > 0 && isBatteryEmpty)
        {
            isBatteryEmpty = false;
            droneMovement.SetMotorsState(true);
        }
    }
}
