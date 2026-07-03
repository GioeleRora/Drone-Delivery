using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float ascendForce = 15f;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSpeed = 5f;
    [SerializeField] private float drag = 3f; // Simulazione dell'attrito dell'aria per frenare il drone

    [Header("Visuals")]
    [Tooltip("Assegna qui il GameObject figlio che contiene il modello 3D del drone")]
    [SerializeField] private Transform droneVisualModel;

    private Rigidbody rb;
    private Vector2 currentMoveInput;
    private float currentAltitudeInput;
    private bool areMotorsActive = true;

    private void Awake()
    {
        // Caching delle reference per evitare GetComponent in Update
        rb = GetComponent<Rigidbody>();
        
        // Ottimizzazioni fisiche per il drone
        rb.useGravity = true; 
        rb.linearDamping = drag;
        rb.angularDamping = drag;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Movimento fluido per la camera
    }

    private void FixedUpdate()
    {
        // Se i motori sono spenti (es. batteria esaurita o altitudine critica), 
        // non applichiamo forze. La gravità (nativa del Rigidbody) lo farà precipitare.
        if (!areMotorsActive) return;

        HandleHovering();
        HandleMovement();
    }

    private void Update()
    {
        HandleTilt();
    }

    /// <summary>
    /// Metodo pubblico per iniettare l'input. Totalmente disaccoppiato dal sistema di input specifico.
    /// In futuro potrà essere chiamato sia da un VirtualJoystick che da una IA (NavMesh).
    /// </summary>
    public void SetInput(Vector2 moveInput, float altitudeInput)
    {
        currentMoveInput = Vector2.ClampMagnitude(moveInput, 1f);
        currentAltitudeInput = Mathf.Clamp(altitudeInput, -1f, 1f);
    }

    /// <summary>
    /// Permette a script esterni (es. BatterySystem o AltitudeManager) di spegnere/accendere i motori.
    /// </summary>
    public void SetMotorsState(bool state)
    {
        areMotorsActive = state;
    }

    private void HandleHovering()
    {
        // Per mantenere il drone in aria opponiamo una forza uguale contraria alla gravità
        // Questo ci permette di avere una fisica reattiva senza usare rb.useGravity = false
        Vector3 counterGravityForce = -Physics.gravity * rb.mass;
        rb.AddForce(counterGravityForce, ForceMode.Force);
    }

    private void HandleMovement()
    {
        // Calcoliamo la forza in base all'orientamento attuale del drone
        Vector3 directionalForce = (transform.forward * currentMoveInput.y + transform.right * currentMoveInput.x) * moveForce;
        Vector3 verticalForce = Vector3.up * currentAltitudeInput * ascendForce;

        // Usiamo AddForce. Questo è vitale perché se un domani uno script "WindReceiver" 
        // applica un'altra AddForce, il motore fisico di Unity le sommerà in automatico e realisticamente.
        rb.AddForce(directionalForce + verticalForce, ForceMode.Force);
    }

    private void HandleTilt()
    {
        // Se non è stato assegnato alcun modello visivo, evitiamo errori
        if (droneVisualModel == null) return;

        float targetPitch = 0f;
        float targetRoll = 0f;

        if (areMotorsActive)
        {
            // Effetto visivo: il modello 3D si inclina nella direzione in cui si muove
            targetPitch = currentMoveInput.y * maxTiltAngle;
            targetRoll = -currentMoveInput.x * maxTiltAngle;
        }

        // Fissiamo lo yaw a 0 (il root gestirà la rotazione Y), per evitare gimbal lock e mantenere coerenza fisica
        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);
        
        // Usiamo Slerp per una rotazione morbida e ottimizzata solo sul modello figlio.
        // Usiamo Time.deltaTime in quanto chiamato in Update (Render visivo).
        droneVisualModel.localRotation = Quaternion.Slerp(droneVisualModel.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }
}
