using UnityEngine;
using System;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [Header("Delivery Components")]
    [Tooltip("Riferimento alla stazione di ritiro pacco.")]
    [SerializeField] private PackageStation packageStation;
    [Tooltip("Riferimento alla zona di consegna pacco.")]
    [SerializeField] private DeliveryZone deliveryZone;
    [Tooltip("Riferimento al drone controllato dal giocatore.")]
    [SerializeField] private DroneMovement drone;

    // Espone il riferimento al drone per altri manager
    public DroneMovement Drone => drone;

    [Header("Package Settings")]
    [Tooltip("Peso aggiuntivo (in kg) da applicare alla Rigidbody del drone quando trasporta un pacco.")]
    [SerializeField] private float packageWeight = 0.5f;
    [Tooltip("Prefab del modello visivo del pacco (Greyboxing - es. un cubo) da attaccare al drone.")]
    [SerializeField] private GameObject packageVisualPrefab;
    [Tooltip("Offset del pacco rispetto al centro del drone.")]
    [SerializeField] private Vector3 packageAttachOffset = new Vector3(0f, -0.5f, 0f);

    public enum DeliveryState
    {
        ReadyToSpawn,
        WaitingForPickup,
        Carrying,
        Delivered
    }

    // State properties
    public DeliveryState CurrentState { get; private set; } = DeliveryState.ReadyToSpawn;
    public bool IsCarryingPackage => CurrentState == DeliveryState.Carrying;
    public int DeliveredPackagesCount { get; private set; } = 0;

    public PackageStation PackageStation => packageStation;
    public DeliveryZone DeliveryZone => deliveryZone;

    // Events for decoupling (e.g. UI or audio systems can listen)
    public event Action OnPackageSpawned;
    public event Action OnPackagePickedUp;
    public event Action OnPackageDelivered;

    private float originalDroneMass = 1.0f;
    private Rigidbody droneRigidbody = null;
    private GameObject activePackageOnDrone = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (drone != null)
        {
            droneRigidbody = drone.GetComponent<Rigidbody>();
            if (droneRigidbody != null)
            {
                originalDroneMass = droneRigidbody.mass;
            }
            else
            {
                Debug.LogError("DeliveryManager: Rigidbody non trovato sul DroneMovement fornito.");
            }
        }
        else
        {
            Debug.LogError("DeliveryManager: DroneMovement non assegnato nell'Inspector.");
        }

        // Registrazione eventi delle stazioni e zone
        if (packageStation != null)
        {
            packageStation.OnPackagePickedUp += HandlePackagePickedUp;
        }
        else
        {
            Debug.LogError("DeliveryManager: PackageStation non assegnata nell'Inspector.");
        }

        if (deliveryZone != null)
        {
            deliveryZone.OnDroneLandedWithPackage += HandlePackageDelivered;
        }
        else
        {
            Debug.LogError("DeliveryManager: DeliveryZone non assegnata nell'Inspector.");
        }

        // Avvio del primo ciclo di consegna
        StartNewDeliveryCycle();
    }

    private void OnDestroy()
    {
        // Rimozione iscrizioni agli eventi per prevenire memory leak
        if (packageStation != null)
        {
            packageStation.OnPackagePickedUp -= HandlePackagePickedUp;
        }

        if (deliveryZone != null)
        {
            deliveryZone.OnDroneLandedWithPackage -= HandlePackageDelivered;
        }
    }

    public void StartNewDeliveryCycle()
    {
        if (packageStation == null) return;

        CurrentState = DeliveryState.WaitingForPickup;

        // Attiva la stazione di spawn del pacco
        packageStation.SpawnPackage();

        // Disattiva la zona di consegna fino a che il pacco non è raccolto
        if (deliveryZone != null)
        {
            deliveryZone.SetZoneActive(false);
        }

        OnPackageSpawned?.Invoke();
        Debug.Log("DeliveryManager: Nuovo pacco spawnato alla stazione di ritiro.");
    }

    private void HandlePackagePickedUp(DroneMovement pickedUpDrone)
    {
        if (CurrentState != DeliveryState.WaitingForPickup) return;

        CurrentState = DeliveryState.Carrying;

        // Aumenta la massa fisica del drone
        if (droneRigidbody != null)
        {
            droneRigidbody.mass = originalDroneMass + packageWeight;
            Debug.Log($"DeliveryManager: Pacco raccolto! Massa drone incrementata a {droneRigidbody.mass}kg. Movimento influenzato.");
        }

        // Instanzia il pacco visivo sotto il drone
        if (packageVisualPrefab != null && activePackageOnDrone == null)
        {
            activePackageOnDrone = Instantiate(packageVisualPrefab, pickedUpDrone.transform);
            activePackageOnDrone.transform.localPosition = packageAttachOffset;
            activePackageOnDrone.transform.localRotation = Quaternion.identity;
        }

        // Attiva la zona di consegna
        if (deliveryZone != null)
        {
            deliveryZone.SetZoneActive(true);
        }

        OnPackagePickedUp?.Invoke();
    }

    private void HandlePackageDelivered(DroneMovement deliveringDrone)
    {
        if (CurrentState != DeliveryState.Carrying) return;

        CurrentState = DeliveryState.Delivered;
        DeliveredPackagesCount++;

        // Ripristina la massa originaria del drone
        if (droneRigidbody != null)
        {
            droneRigidbody.mass = originalDroneMass;
            Debug.Log($"DeliveryManager: Pacco consegnato con successo! Massa drone ripristinata a {originalDroneMass}kg.");
        }

        // Rimuove il pacco visivo dal drone
        if (activePackageOnDrone != null)
        {
            Destroy(activePackageOnDrone);
            activePackageOnDrone = null;
        }

        // Disattiva la zona di consegna
        if (deliveryZone != null)
        {
            deliveryZone.SetZoneActive(false);
        }

        OnPackageDelivered?.Invoke();

        // Avvia automaticamente un nuovo ciclo
        StartNewDeliveryCycle();
    }
}
