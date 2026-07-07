using UnityEngine;

public class CompassHUD : MonoBehaviour
{
    public Transform drone;
    public RectTransform compassTape;
    public float pixelsPerDegree = 2f;

    private void Update()
    {
        if (drone != null && compassTape != null)
        {
            // Muove il nastro lungo l'asse X in base all'angolo Y del drone
            compassTape.anchoredPosition = new Vector2(-drone.eulerAngles.y * pixelsPerDegree, 0);
        }
    }
}
