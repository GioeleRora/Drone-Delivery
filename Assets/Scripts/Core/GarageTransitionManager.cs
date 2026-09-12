using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class GarageTransitionManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameObject garageInstance;       // La stanza a -5000
    public GameObject mapManager;           // L'ambiente procedurale
    public GameObject hubCanvas;            // La UI del garage
    public GameObject flightCanvas;         // La UI di volo (bussola, altitudine)
    public Transform droneTransform;        // Il drone
    public Transform garagePedestalPoint;   // Punto esatto di spawn nel garage
    public DroneMovement droneMovement;     // Script per sbloccare il volo
    public CameraFollow cameraFollow;       // Script telecamera

    [Header("Deploy Settings")]
    public Transform surfaceExitPoint;      // Punto di uscita nel mondo reale
    public float holdToDeployTime = 1.5f;   // Secondi necessari per il Deploy

    public UnityEvent OnDeployStarted;
    public UnityEvent OnDeployCompleted;

    private float currentHoldTime = 0f;
    private bool isDeploying = false;
    private bool inGarage = true;           // Stato iniziale: siamo nel garage?

    private void Start()
    {
        // Se il gioco inizia nel garage, impostiamo la scena correttamente
        if (inGarage)
        {
            EnterGarageMode();
        }
    }

    private void Update()
    {
        if (!inGarage || isDeploying) return;

        Gamepad gamepad = Gamepad.current;
        Keyboard kb = Keyboard.current;

        bool deployPressed = false;

        // Bottone Y (North) o tasto H (Hold)
        if (gamepad != null && gamepad.buttonNorth.isPressed) deployPressed = true;
        if (kb != null && kb.hKey.isPressed) deployPressed = true;

        if (deployPressed)
        {
            currentHoldTime += Time.deltaTime;
            // TODO: Qui potremmo aggiungere un feedback UI che si riempie
            if (currentHoldTime >= holdToDeployTime)
            {
                ExecuteDeploy();
            }
        }
        else
        {
            currentHoldTime = 0f; // Resetta se il giocatore molla il tasto
        }
    }

    public void EnterGarageMode()
    {
        AutoFindReferences();
        inGarage = true;
        
        // Spegne il mondo pesante e accende l'istanza isolata
        if (mapManager != null) mapManager.SetActive(false);
        if (flightCanvas != null) flightCanvas.SetActive(false); // Nasconde UI Volo
        if (garageInstance != null) garageInstance.SetActive(true);
        if (hubCanvas != null) hubCanvas.SetActive(true);

        // Blocca il drone sul piedistallo
        if (droneMovement != null) droneMovement.SetMotorsState(false);
        if (cameraFollow != null) cameraFollow.gameObject.SetActive(false); // Spegniamo completamente la Main Camera
        
        if (droneTransform != null)
        {
            // Teletrasporto al piedistallo (usa il punto predefinito o calcolalo dal piedistallo)
            if (garagePedestalPoint != null)
            {
                droneTransform.position = garagePedestalPoint.position;
                droneTransform.rotation = garagePedestalPoint.rotation;
            }
            else
            {
                // Fallback dinamico: se manca il punto esatto, cerca l'oggetto piedistallo e posizionati sopra
                GameObject targetPedestal = null;
                foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (go.name.ToLower().Contains("pedestal"))
                    {
                        targetPedestal = go;
                        break;
                    }
                }
                
                if (targetPedestal != null)
                {
                    droneTransform.position = targetPedestal.transform.position + new Vector3(0, 0.5f, 0);
                    droneTransform.rotation = targetPedestal.transform.rotation;
                }
            }
            
            // Ferma completamente la fisica (gravità disattivata)
            Rigidbody rb = droneTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }

    public void ExecuteDeploy()
    {
        isDeploying = true;
        OnDeployStarted?.Invoke();

        // 1. Spegne UI e Garage, Accende Mondo Procedurale
        if (hubCanvas != null) hubCanvas.SetActive(false);
        if (garageInstance != null) garageInstance.SetActive(false);
        if (mapManager != null) mapManager.SetActive(true);
        if (flightCanvas != null) flightCanvas.SetActive(true); // Mostra UI Volo

        // 2. Teletrasporto istantaneo all'uscita (senza caricamenti!)
        if (droneTransform != null && surfaceExitPoint != null)
        {
            droneTransform.position = surfaceExitPoint.position;
            droneTransform.rotation = surfaceExitPoint.rotation;
        }

        // 3. Ripristina il controllo (Volo e Telecamera)
        if (droneMovement != null) droneMovement.SetMotorsState(true);
        if (droneTransform != null)
        {
            Rigidbody rb = droneTransform.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false; // Riattiva la gravità e la fisica
        }
        
        if (cameraFollow != null)
        {
            cameraFollow.gameObject.SetActive(true);
            cameraFollow.enabled = true;
            // Snap immediato della camera dietro al drone
            cameraFollow.transform.position = droneTransform.position + cameraFollow.offset;
        }

        inGarage = false;
        isDeploying = false;
        currentHoldTime = 0f;
        
        OnDeployCompleted?.Invoke();
        Debug.Log("Deploy Eseguito: Nessun caricamento. Benvenuto nel cratere!");
    }

    private void AutoFindReferences()
    {
        if (droneTransform == null)
        {
            GameObject droneObj = GameObject.Find("Drone_Root");
            if (droneObj != null)
            {
                droneTransform = droneObj.transform;
                droneMovement = droneObj.GetComponent<DroneMovement>();
            }
        }
        
        if (garageInstance == null)
        {
            GameObject garageObj = GameObject.Find("GarageInstance");
            if (garageObj != null) garageInstance = garageObj;
        }
        
        if (mapManager == null)
        {
            GameObject mapObj = GameObject.Find("MapManager");
            if (mapObj != null) mapManager = mapObj;
            else 
            {
                // Fallback name used in the user's project
                GameObject voxelObj = GameObject.Find("VoxelCraterContainer");
                if (voxelObj != null) mapManager = voxelObj;
            }
        }
        
        if (flightCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null) flightCanvas = canvasObj;
        }
    }
}
