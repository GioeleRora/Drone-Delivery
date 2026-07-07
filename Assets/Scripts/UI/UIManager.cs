using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Tooltip("Il Transform del drone da cui leggere l'altitudine")]
    [SerializeField] private Transform droneTransform;
    
    [Tooltip("Il testo dell'interfaccia in cui mostrare il valore dell'altitudine")]
    [SerializeField] private TextMeshProUGUI altitudeText;

    [Header("Motors Status")]
    [Tooltip("Riferimento allo script di movimento del drone")]
    [SerializeField] private DroneMovement droneMovement;

    [SerializeField] private UnityEngine.UI.Image batteryFill;
    [SerializeField] private TextMeshProUGUI batteryPercentage;

    private void Update()
    {
        // Controllo di sicurezza per evitare NullReferenceException sull'altitudine
        if (droneTransform != null && altitudeText != null)
        {
            int altitude = Mathf.RoundToInt(droneTransform.position.y);
            altitudeText.text = "Altitudine: " + altitude + " m";
        }

        // Controllo della batteria
        if (droneMovement != null && batteryFill != null && batteryPercentage != null)
        {
            BatterySystem battery = droneMovement.GetComponent<BatterySystem>();
            if (battery != null)
            {
                float fill = battery.CurrentBattery / battery.MaxBattery;
                batteryFill.fillAmount = fill;
                batteryFill.color = Color.Lerp(Color.red, Color.green, fill);
                batteryPercentage.text = Mathf.RoundToInt(fill * 100) + "%";
            }
        }
    }

    public void OnToggleMotorsClicked()
    {
        if (droneMovement != null)
        {
            droneMovement.ToggleMotors();
        }
    }
}
