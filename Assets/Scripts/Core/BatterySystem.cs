using UnityEngine;

public class BatterySystem : MonoBehaviour
{
    [Tooltip("Capacità massima della batteria")]
    [SerializeField] private float maxBattery = 100f;
    
    [Tooltip("Quantità di batteria consumata al secondo")]
    [SerializeField] private float drainRate = 5f;

    public float MaxBattery => maxBattery;
    public float DrainMultiplier { get; set; } = 1f;

    // Proprietà pubblica in sola lettura per l'esterno
    public float CurrentBattery { get; private set; }

    // Proprietà calcolata per sapere se è scarica
    public bool IsDepleted => CurrentBattery <= 0f;

    private void Awake()
    {
        CurrentBattery = maxBattery;
    }

    private void Update()
    {
        // Se non è già scarica, continuiamo a prosciugarla
        if (!IsDepleted)
        {
            // Usiamo Mathf.Max per evitare che il valore scenda in negativo
            CurrentBattery = Mathf.Max(0f, CurrentBattery - drainRate * DrainMultiplier * Time.deltaTime);
        }
    }
}
