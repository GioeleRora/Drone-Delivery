using UnityEngine;
using UnityEngine.InputSystem;

public class HubCameraDirector : MonoBehaviour
{
    [Header("Camera Targets")]
    public Transform officinaTarget;
    public Transform negozioTarget;
    public Transform impostazioniTarget;

    [Header("Settings")]
    public float transitionSpeed = 5f;
    public float orbitSensitivity = 50f;
    public float orbitMaxAngle = 30f;

    private HubUIManager.HubTab currentTab = HubUIManager.HubTab.Officina;
    private float currentOrbitYaw = 0f;

    public void ChangeTab(HubUIManager.HubTab newTab)
    {
        currentTab = newTab;
        currentOrbitYaw = 0f; // Resetta l'orbita quando si cambia tab
    }

    private void LateUpdate()
    {
        Transform targetAnchor = GetCurrentTarget();
        if (targetAnchor == null) return;

        Vector3 targetPos = targetAnchor.position;
        Quaternion targetRot = targetAnchor.rotation;

        // Effetto Orbita 3D solo nella tab Officina
        if (currentTab == HubUIManager.HubTab.Officina)
        {
            float orbitInput = 0f;
            
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                orbitInput = gamepad.rightStick.x.ReadValue();
            }
            
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.dKey.isPressed) orbitInput = 1f;
                if (kb.aKey.isPressed) orbitInput = -1f;
            }

            currentOrbitYaw += orbitInput * orbitSensitivity * Time.deltaTime;
            currentOrbitYaw = Mathf.Clamp(currentOrbitYaw, -orbitMaxAngle, orbitMaxAngle);

            // Applica l'orbita attorno al punto focale (assumiamo che il focus sia [0,0,0] relativo al target, ma per semplicità ruotiamo la camera sul posto)
            targetRot *= Quaternion.Euler(0, currentOrbitYaw, 0);
        }

        // Lerp fluido
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * transitionSpeed);
    }

    private Transform GetCurrentTarget()
    {
        switch (currentTab)
        {
            case HubUIManager.HubTab.Officina: return officinaTarget;
            case HubUIManager.HubTab.Negozio: return negozioTarget;
            case HubUIManager.HubTab.Impostazioni: return impostazioniTarget;
            default: return null;
        }
    }
}
