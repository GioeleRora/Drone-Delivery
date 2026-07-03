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
        rb.drag = drag;
        rb.angularDrag = drag;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Movimento fluido per la camera
    }

    private void FixedUpdate()
    {
        // Se i motori sono spenti (es. batteria esaurita o altitudine critica), 
        // non applichiamo forze. La gravità (nativa del Rigidbody) lo farà precipitare.
        if (!areMotorsActive) return;

        HandleHovering();
        HandleMovement();
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
        // Effetto visivo/fisico: il drone si inclina nella direzione in cui si muove
        float targetPitch = currentMoveInput.y * maxTiltAngle;
        float targetRoll = -currentMoveInput.x * maxTiltAngle;

        // Manteniamo lo yaw (rotazione Y) attuale intatto per ora
        Quaternion targetRotation = Quaternion.Euler(targetPitch, transform.eulerAngles.y, targetRoll);
        
        // Usiamo Slerp per una rotazione morbida e ottimizzata
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * tiltSpeed);
    }
}
