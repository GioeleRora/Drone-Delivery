using UnityEngine;

[RequireComponent(typeof(DroneMovement))]
public class DroneInputHandler : MonoBehaviour
{
    private DroneMovement droneMovement;
    
    // Stato degli input Mobile
    private Vector2 mobileMoveInput;
    private bool isMobileAscending;
    private bool isMobileDescending;

    private void Awake()
    {
        droneMovement = GetComponent<DroneMovement>();
    }

    // Questi metodi verranno chiamati direttamente dai bottoni UI Mobile (EventTrigger: PointerDown/Up)
    public void SetAscendInput(bool isAscending)
    {
        isMobileAscending = isAscending;
    }

    public void SetDescendInput(bool isDescending)
    {
        isMobileDescending = isDescending;
    }
    
    // Questo verrà chiamato da uno script Joystick Virtuale
    public void ReceiveJoystickInput(Vector2 joystickDirection)
    {
        mobileMoveInput = joystickDirection;
    }

    private void Update()
    {
        // 1. Calcolo input Altitudine (Mobile)
        float currentAltitudeInput = 0f;
        if (isMobileAscending) currentAltitudeInput += 1f;
        if (isMobileDescending) currentAltitudeInput -= 1f;

        // 2. Calcolo input Movimento (Mobile)
        Vector2 currentMoveInput = mobileMoveInput;

        // 3. Fallback per testare su PC comodamente dall'Editor
        #if UNITY_EDITOR || UNITY_STANDALONE
        Vector2 pcMoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (pcMoveInput != Vector2.zero) 
        {
            currentMoveInput = pcMoveInput;
        }
        
        // Applichiamo l'input da tastiera per l'altitudine solo se i bottoni UI non sono premuti
        if (currentAltitudeInput == 0f)
        {
            if (Input.GetKey(KeyCode.Space)) 
            {
                currentAltitudeInput = 1f;
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.C)) 
            {
                currentAltitudeInput = -1f;
            }
        }
        #endif

        // 4. Inviamo l'input calcolato al motore fisico
        droneMovement.SetInput(currentMoveInput, currentAltitudeInput);
    }
}
