using UnityEngine;

[RequireComponent(typeof(DroneMovement))]
public class DroneInputHandler : MonoBehaviour
{
    private DroneMovement droneMovement;

    // Se in futuro passeremo al New Input System o a un Joystick UI,
    // cambieremo solo il modo in cui queste due variabili vengono lette.
    private Vector2 moveInput;
    private float altitudeInput;

    private void Awake()
    {
        droneMovement = GetComponent<DroneMovement>();
    }

    private void Update()
    {
        // 1. Lettura dell'Input (PC testing: frecce direzionali / WASD)
        // Per il mobile, in futuro, sostituiremo GetAxis con le API del Virtual Joystick (es. joystick.Horizontal)
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        // Per l'altitudine usiamo ad esempio Spazio per salire, Shift o C per scendere
        // Su mobile potrebbero essere dei pulsanti a schermo "Ascend" e "Descend"
        altitudeInput = 0f;
        if (Input.GetKey(KeyCode.Space))
        {
            altitudeInput = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.C))
        {
            altitudeInput = -1f;
        }

        // 2. Inviamo l'input al controller di movimento
        droneMovement.SetInput(moveInput, altitudeInput);
    }
}
