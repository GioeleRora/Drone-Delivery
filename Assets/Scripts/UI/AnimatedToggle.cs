using UnityEngine;
using UnityEngine.UI;

public class AnimatedToggle : MonoBehaviour
{
    [SerializeField] private DroneMovement droneMovement;
    [SerializeField] private RectTransform handle;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;
    [SerializeField] private float animationSpeed = 15f;

    private void Update()
    {
        if (droneMovement == null || handle == null || backgroundImage == null) return;

        bool isOn = droneMovement.AreMotorsOn;
        float targetX = isOn ? 25f : -25f;
        Color targetColor = isOn ? onColor : offColor;

        // Anima la posizione
        Vector2 currentPos = handle.anchoredPosition;
        currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * animationSpeed);
        handle.anchoredPosition = currentPos;

        // Anima il colore
        backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, Time.deltaTime * animationSpeed);
    }

    public void OnToggleClicked()
    {
        if (droneMovement != null)
        {
            droneMovement.ToggleMotors();
        }
    }
}
