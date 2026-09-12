using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Header("Freelook Settings")]
    public float freeLookSensitivity = 150f;
    public float freeLookReturnSpeed = 5f;

    private Vector3 _velocity = Vector3.zero;
    private float _freeLookYaw = 0f;
    private float _freeLookPitch = 0f;

    private void LateUpdate()
    {
        if (target != null)
        {
            // --- GESTIONE FREELOOK ---
            bool isFreelook = false;
            float lookX = 0f;
            float lookY = 0f;

            Gamepad gamepad = UnityEngine.InputSystem.Gamepad.current;
            UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;

            if (gamepad != null && gamepad.leftShoulder.isPressed) isFreelook = true;
            if (kb != null && kb.leftAltKey.isPressed) isFreelook = true;

            if (isFreelook)
            {
                if (kb != null)
                {
                    if (kb.rightArrowKey.isPressed) lookX = 1f;
                    if (kb.leftArrowKey.isPressed) lookX = -1f;
                    if (kb.upArrowKey.isPressed) lookY = 1f;
                    if (kb.downArrowKey.isPressed) lookY = -1f;
                }

                if (gamepad != null)
                {
                    Vector2 rightStick = gamepad.rightStick.ReadValue();
                    if (Mathf.Abs(rightStick.x) > 0.1f) lookX = rightStick.x;
                    if (Mathf.Abs(rightStick.y) > 0.1f) lookY = rightStick.y;
                }
            }

            if (isFreelook)
            {
                _freeLookYaw += lookX * freeLookSensitivity * Time.deltaTime;
                _freeLookPitch -= lookY * freeLookSensitivity * Time.deltaTime; // Sottraiamo per invertire correttamente il pitch (su guarda in alto)
                _freeLookPitch = Mathf.Clamp(_freeLookPitch, -45f, 60f); // Limite per evitare ribaltamenti
            }
            else
            {
                // Ritorno morbido al centro
                _freeLookYaw = Mathf.Lerp(_freeLookYaw, 0f, freeLookReturnSpeed * Time.deltaTime);
                _freeLookPitch = Mathf.Lerp(_freeLookPitch, 0f, freeLookReturnSpeed * Time.deltaTime);
            }

            // 1. Calcolo Posizione Desiderata (Ora influenzata dal Freelook)
            Quaternion freeLookRot = Quaternion.Euler(_freeLookPitch, _freeLookYaw, 0f);
            Vector3 desiredPosition = target.position + (target.rotation * freeLookRot) * offset;
            
            // 2. Molla Progressiva (Dynamic SmoothTime)
            float currentLagDistance = (transform.position - desiredPosition).magnitude;
            
            // Distanza a cui l'elastico raggiunge la sua massima tensione
            // (Abbassato da 2.0 a 1.5 per farlo intervenire molto prima)
            float maxTensionDistance = 1.5f; 
            
            // Calcoliamo una percentuale (da 0 a 1) di quanto è "teso" l'elastico
            float t = Mathf.Clamp01(currentLagDistance / maxTensionDistance);
            
            // Curva di tensione: Usiamo SmoothStep (Curva a S).
            // Rispetto a prima, l'indurimento inizia subito ed è molto più aggressivo
            // a metà percorso, bloccando il drone molto prima che si allontani troppo.
            t = Mathf.SmoothStep(0f, 1f, t); 
            
            // Tempo di reazione base (morbido)
            float baseSmoothTime = 1f / smoothSpeed; 
            // Tempo di reazione sotto sforzo (elastico rigidissimo)
            float stiffSmoothTime = 0.015f; 
            
            float dynamicSmoothTime = Mathf.Lerp(baseSmoothTime, stiffSmoothTime, t);

            // 3. Spostamento Fisico 
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, dynamicSmoothTime);
            
            // 2. Calcolo Pitch (Inclinazione Su/Giù)
            // Calcoliamo dinamicamente l'angolo per mantenere il drone sempre centrato in verticale
            float dy = target.position.y - transform.position.y;
            float distanceXZ = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(target.position.x, target.position.z));
            float targetPitch = -Mathf.Atan2(dy, distanceXZ) * Mathf.Rad2Deg;
            
            // 3. Calcolo Yaw (Rotazione Destra/Sinistra)
            // Aggiungiamo l'angolo di visuale libera alla rotazione base del drone
            float targetYaw = target.eulerAngles.y + _freeLookYaw;
            
            // 4. Applichiamo la rotazione in modo fluido
            float currentPitch = transform.eulerAngles.x;
            float currentYaw = transform.eulerAngles.y;
            
            float smoothedPitch = Mathf.LerpAngle(currentPitch, targetPitch, smoothSpeed * Time.deltaTime);
            float smoothedYaw = Mathf.LerpAngle(currentYaw, targetYaw, smoothSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(smoothedPitch, smoothedYaw, 0f);
        }
    }
}
