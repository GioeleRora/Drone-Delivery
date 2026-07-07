using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    public Vector2 InputVector { get; private set; }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            // Normalizza in base alle dimensioni del background
            localPoint.x = localPoint.x / (background.sizeDelta.x / 2f);
            localPoint.y = localPoint.y / (background.sizeDelta.y / 2f);

            InputVector = new Vector2(localPoint.x, localPoint.y);
            
            // Clamp a magnitudo 1 (cerchio perfetto)
            InputVector = (InputVector.magnitude > 1.0f) ? InputVector.normalized : InputVector;

            // Muovi l'handle
            if (handle != null)
            {
                handle.anchoredPosition = new Vector2(
                    InputVector.x * (background.sizeDelta.x / 2f),
                    InputVector.y * (background.sizeDelta.y / 2f)
                );
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}
