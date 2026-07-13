using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class DroneMovement : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("PC Inputs: WASD (Pitch/Roll), Space/Shift (Throttle), Arrows (Yaw)")]
    // Virtual Joysticks removed for PC transition

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float ascendForce = 15f;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSpeed = 5f;
    [SerializeField] private float drag = 3f; // Simulazione dell'attrito dell'aria per frenare il drone
    [SerializeField] private float yawSpeed = 300f;

    [Header("Visuals")]
    [Tooltip("Assegna qui il GameObject figlio che contiene il modello 3D del drone")]
    [SerializeField] private Transform droneVisualModel;

    private Rigidbody rb;
    private BatterySystem batterySystem;
    private DroneHealth droneHealth;
    private Vector2 currentMoveInput;
    private float currentAltitudeInput;
    private float currentYawInput;
    
    public bool AreMotorsOn { get; private set; } = false;
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        // Aggiunge automaticamente i componenti fisici necessari se non presenti
        if (GetComponent<DroneHealth>() == null)
        {
            gameObject.AddComponent<DroneHealth>();
            Debug.Log("DroneMovement: Componente DroneHealth mancante sul drone. Aggiunto automaticamente.");
        }
        if (GetComponent<WindReceiver>() == null)
        {
            gameObject.AddComponent<WindReceiver>();
            Debug.Log("DroneMovement: Componente WindReceiver mancante sul drone. Aggiunto automaticamente.");
        }

        rb = GetComponent<Rigidbody>();
        batterySystem = GetComponent<BatterySystem>();
        droneHealth = GetComponent<DroneHealth>();
        
        rb.useGravity = true; 
        rb.linearDamping = drag;
        rb.angularDamping = drag;
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if ((batterySystem != null && batterySystem.IsDepleted) || (droneHealth != null && droneHealth.IsDead))
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

        // Rotazione fluida (Imbardata) gestita dalla fisica per evitare stuttering
        Quaternion deltaRotation = Quaternion.Euler(0, currentYawInput * yawSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private void Update()
    {
        if (AreMotorsOn)
        {
            // Controllo Freelook
            bool isFreelook = Input.GetKey(KeyCode.LeftAlt);
#if ENABLE_INPUT_SYSTEM
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.leftShoulder.isPressed) isFreelook = true;
#endif

            // PC Controls - Mode 2 Drone Standard
            float pitchInput = 0f;
            float rollInput = 0f;
            
            // Disabilitiamo il beccheggio e rollio se stiamo guardando in giro
            if (!isFreelook)
            {
                pitchInput = Input.GetKey(KeyCode.UpArrow) ? 1f : (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f);
                rollInput = Input.GetKey(KeyCode.RightArrow) ? 1f : (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f);
            }
            
            float throttleInput = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);
            float yawInput = Input.GetKey(KeyCode.D) ? 1f : (Input.GetKey(KeyCode.A) ? -1f : 0f);

#if ENABLE_INPUT_SYSTEM
            if (gamepad != null)
            {
                // Mode 2 Standard:
                // Left Stick: Throttle (Y) / Yaw (X)
                // Right Stick: Pitch (Y) / Roll (X) (Solo se Freelook non è attivo)
                Vector2 leftStick = gamepad.leftStick.ReadValue();
                Vector2 rightStick = gamepad.rightStick.ReadValue();
                
                if (Mathf.Abs(leftStick.y) > 0.1f) throttleInput = leftStick.y;
                if (Mathf.Abs(leftStick.x) > 0.1f) yawInput = leftStick.x;
                
                if (!isFreelook)
                {
                    if (Mathf.Abs(rightStick.y) > 0.1f) pitchInput = rightStick.y;
                    if (Mathf.Abs(rightStick.x) > 0.1f) rollInput = rightStick.x;
                }
            }
#endif

            SetInput(new Vector2(rollInput, pitchInput), throttleInput, yawInput);
        }
        else
        {
            SetInput(Vector2.zero, 0f, 0f);
        }
        
        HandleTilt();
    }

    public void ToggleMotors()
    {
        if (droneHealth != null && droneHealth.IsDead)
        {
            AreMotorsOn = false;
            return;
        }

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
    public void SetInput(Vector2 moveInput, float altitudeInput, float yawInput)
    {
        currentMoveInput = Vector2.ClampMagnitude(moveInput, 1f);
        currentAltitudeInput = Mathf.Clamp(altitudeInput, -1f, 1f);
        currentYawInput = Mathf.Clamp(yawInput, -1f, 1f);
    }

    /// <summary>
    /// Permette a script esterni (es. BatterySystem o AltitudeManager) di spegnere/accendere i motori.
    /// </summary>
    public void SetMotorsState(bool state)
    {
        if (droneHealth != null && droneHealth.IsDead && state == true)
        {
            AreMotorsOn = false;
            return;
        }

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

        // Tetto massimo rimosso: Esplorazione profonda e verticale libera!
        Vector3 verticalForce = Vector3.up * finalAltitudeInput * ascendForce;

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
