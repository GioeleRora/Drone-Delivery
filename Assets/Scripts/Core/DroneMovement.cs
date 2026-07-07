using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneMovement : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool useKeyboardInput = true;

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float ascendForce = 15f;
    [SerializeField] private float maxAltitude = 20f;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSpeed = 5f;
    [SerializeField] private float drag = 3f; // Simulazione dell'attrito dell'aria per frenare il drone
    [SerializeField] private float yawSpeed = 300f;

    [Header("Visuals")]
    [Tooltip("Assegna qui il GameObject figlio che contiene il modello 3D del drone")]
    [SerializeField] private Transform droneVisualModel;

    private Rigidbody rb;
    private BatterySystem batterySystem;
    private Vector2 currentMoveInput;
    private float currentAltitudeInput;
    
    private VirtualJoystick leftJoystick;
    private VirtualJoystick rightJoystick;
    
    public bool AreMotorsOn { get; private set; } = false;
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        batterySystem = GetComponent<BatterySystem>();
        
        VirtualJoystick[] joysticks = UnityEngine.Object.FindObjectsByType<VirtualJoystick>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var j in joysticks)
        {
            if (j.name.Contains("Left")) leftJoystick = j;
            if (j.name.Contains("Right")) rightJoystick = j;
        }
        
        rb.useGravity = true; 
        rb.linearDamping = drag;
        rb.angularDamping = drag;
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (batterySystem != null && batterySystem.IsDepleted)
        {
            AreMotorsOn = false;
        }

        IsGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (batterySystem != null)
        {
            if (!AreMotorsOn)
            {
                batterySystem.DrainMultiplier = 0f;
            }
            else if (IsGrounded)
            {
                batterySystem.DrainMultiplier = 0.1f;
            }
            else
            {
                batterySystem.DrainMultiplier = 1f;
            }
        }

        if (!AreMotorsOn) return;

        HandleHovering();
        HandleMovement();
    }

    private void Update()
    {
        if (AreMotorsOn)
        {
            float pitchInput = useKeyboardInput ? (Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f)) : rightJoystick.InputVector.y;
            float rollInput = useKeyboardInput ? (Input.GetKey(KeyCode.D) ? 1f : (Input.GetKey(KeyCode.A) ? -1f : 0f)) : leftJoystick.InputVector.x;
            float throttleInput = useKeyboardInput ? (Input.GetKey(KeyCode.Space) ? 1f : (Input.GetKey(KeyCode.LeftShift) ? -1f : 0f)) : leftJoystick.InputVector.y;
            float yawInput = useKeyboardInput ? (Input.GetKey(KeyCode.RightArrow) ? 1f : (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f)) : rightJoystick.InputVector.x;

            SetInput(new Vector2(rollInput, pitchInput), throttleInput);
            transform.Rotate(0, yawInput * yawSpeed * Time.deltaTime, 0);
        }
        else
        {
            SetInput(Vector2.zero, 0f);
        }
        
        HandleTilt();
    }

    public void ToggleMotors()
    {
        if (batterySystem != null && batterySystem.IsDepleted)
        {
            AreMotorsOn = false;
        }
        else
        {
            AreMotorsOn = !AreMotorsOn;
        }
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
        if (batterySystem != null && batterySystem.IsDepleted && state == true)
        {
            AreMotorsOn = false;
        }
        else
        {
            AreMotorsOn = state;
        }
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
        
        float finalAltitudeInput = currentAltitudeInput;

        // 1. Blocco della spinta: se superiamo il limite e stiamo cercando di salire, forziamo l'input a 0
        if (transform.position.y >= maxAltitude && finalAltitudeInput > 0f)
        {
            finalAltitudeInput = 0f;
        }

        Vector3 verticalForce = Vector3.up * finalAltitudeInput * ascendForce;

        // Usiamo AddForce. Questo è vitale perché se un domani uno script "WindReceiver" 
        // applica un'altra AddForce, il motore fisico di Unity le sommerà in automatico e realisticamente.
        rb.AddForce(directionalForce + verticalForce, ForceMode.Force);

        // 2. Controllo di sicurezza per l'inerzia: azzeriamo la velocità Y (usando linearVelocity per Unity 6)
        if (transform.position.y >= maxAltitude && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    private void HandleTilt()
    {
        // Se non è stato assegnato alcun modello visivo, evitiamo errori
        if (droneVisualModel == null) return;

        float targetPitch = 0f;
        float targetRoll = 0f;

        if (AreMotorsOn)
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
