using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver_Crash,
    GameOver_Battery,
    Victory
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Victory Settings")]
    [Tooltip("Numero di consegne necessarie per vincere la partita")]
    [SerializeField] private int deliveryWinQuota = 3;

    public GameState CurrentState { get; private set; } = GameState.Playing;

    public event Action<GameState> OnGameStateChanged;

    private DroneHealth droneHealth;
    private BatterySystem batterySystem;

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
        CurrentState = GameState.Playing;

        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnPackageDelivered += HandlePackageDelivered;

            DroneMovement drone = DeliveryManager.Instance.Drone;
            if (drone != null)
            {
                droneHealth = drone.GetComponent<DroneHealth>();
                if (droneHealth != null)
                {
                    droneHealth.OnDroneDestroyed += HandleDroneDestroyed;
                }
                else
                {
                    Debug.LogError("GameStateManager: DroneHealth non trovato sul Drone.");
                }

                batterySystem = drone.GetComponent<BatterySystem>();
                if (batterySystem != null)
                {
                    batterySystem.OnBatteryDepleted += HandleBatteryDepleted;
                }
                else
                {
                    Debug.LogError("GameStateManager: BatterySystem non trovato sul Drone.");
                }
            }
            else
            {
                Debug.LogError("GameStateManager: Drone non trovato in DeliveryManager.");
            }
        }
        else
        {
            Debug.LogError("GameStateManager: DeliveryManager.Instance è nullo in Start.");
        }
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnPackageDelivered -= HandlePackageDelivered;
        }
        if (droneHealth != null)
        {
            droneHealth.OnDroneDestroyed -= HandleDroneDestroyed;
        }
        if (batterySystem != null)
        {
            batterySystem.OnBatteryDepleted -= HandleBatteryDepleted;
        }
    }

    private void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        if (CurrentState != GameState.Playing) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);

        // Ferma il tempo fisico della scena per prevenire ulteriori movimenti a partita finita
        if (CurrentState != GameState.Playing)
        {
            Time.timeScale = 0f;
        }
    }

    private void HandleDroneDestroyed()
    {
        ChangeState(GameState.GameOver_Crash);
    }

    private void HandleBatteryDepleted()
    {
        ChangeState(GameState.GameOver_Battery);
    }

    private void HandlePackageDelivered()
    {
        if (DeliveryManager.Instance != null)
        {
            if (DeliveryManager.Instance.DeliveredPackagesCount >= deliveryWinQuota)
            {
                ChangeState(GameState.Victory);
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
