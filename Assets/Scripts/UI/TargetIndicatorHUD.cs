using UnityEngine;

public class TargetIndicatorHUD : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the drone transform. If left empty, will try to fetch it from CompassHUD or fallback to finding DroneMovement.")]
    [SerializeField] private Transform droneTransform;

    [Tooltip("The main camera used for screen projection.")]
    [SerializeField] private Camera mainCamera;

    [Header("Compass Target Marker")]
    [Tooltip("Reference to the Compass HUD script to synchronize settings.")]
    [SerializeField] private CompassHUD compassHUD;

    [Tooltip("The RectTransform of the target marker on the compass tape (must be a child of the tape).")]
    [SerializeField] private RectTransform compassTargetMarker;

    [Header("On-Screen Target Indicator")]
    [Tooltip("The RectTransform of the target indicator UI element on screen.")]
    [SerializeField] private RectTransform onScreenIndicator;

    private Camera cachedCamera;
    private float pixelsPerDegree = 2f;

    private void Start()
    {
        // Cache camera
        cachedCamera = mainCamera != null ? mainCamera : Camera.main;
        if (cachedCamera == null)
        {
            Debug.LogError("TargetIndicatorHUD: Main Camera non trovata!");
        }

        // Cache droneTransform if not assigned
        if (droneTransform == null)
        {
            if (compassHUD != null && compassHUD.Drone != null)
            {
                droneTransform = compassHUD.Drone;
            }
            else
            {
                DroneMovement droneObj = UnityEngine.Object.FindAnyObjectByType<DroneMovement>();
                if (droneObj != null)
                {
                    droneTransform = droneObj.transform;
                }
            }
        }

        // Cache pixelsPerDegree from CompassHUD
        if (compassHUD != null)
        {
            pixelsPerDegree = compassHUD.PixelsPerDegree;
        }

        // Ensure indicators start disabled if no target is active
        if (onScreenIndicator != null)
        {
            onScreenIndicator.gameObject.SetActive(false);
        }
        if (compassTargetMarker != null)
        {
            compassTargetMarker.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (DeliveryManager.Instance == null || droneTransform == null) return;

        // 1. Determine active target position
        Vector3 targetPosition = Vector3.zero;
        bool hasTarget = false;

        DeliveryManager.DeliveryState state = DeliveryManager.Instance.CurrentState;
        if (state == DeliveryManager.DeliveryState.WaitingForPickup)
        {
            var station = DeliveryManager.Instance.PackageStation;
            if (station != null)
            {
                targetPosition = station.transform.position;
                hasTarget = true;
            }
        }
        else if (state == DeliveryManager.DeliveryState.Carrying)
        {
            var zone = DeliveryManager.Instance.DeliveryZone;
            if (zone != null)
            {
                targetPosition = zone.transform.position;
                hasTarget = true;
            }
        }

        if (!hasTarget)
        {
            // Hide indicators if there's no active target
            if (onScreenIndicator != null && onScreenIndicator.gameObject.activeSelf)
            {
                onScreenIndicator.gameObject.SetActive(false);
            }
            if (compassTargetMarker != null && compassTargetMarker.gameObject.activeSelf)
            {
                compassTargetMarker.gameObject.SetActive(false);
            }
            return;
        }

        // 2. Compass Target Marker Logic
        if (compassTargetMarker != null)
        {
            if (!compassTargetMarker.gameObject.activeSelf)
            {
                compassTargetMarker.gameObject.SetActive(true);
            }

            // Calculate direction from drone to target (in horizontal plane XZ)
            Vector3 direction = targetPosition - droneTransform.position;
            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            
            // Format target angle to be between 0 and 360
            float targetHeading = (targetYaw + 360f) % 360f;

            // Get the drone's current heading from the drone's rotation
            float droneHeading = droneTransform.eulerAngles.y;
            float wrappedDroneHeading = ((droneHeading % 360f) + 360f) % 360f;

            // Find the shortest angular difference to prevent visual jumping when wrapping
            float diff = Mathf.DeltaAngle(wrappedDroneHeading, targetHeading);
            float wrappedTargetHeading = wrappedDroneHeading + diff;

            // Position the marker on the compass tape
            float markerLocalX = wrappedTargetHeading * pixelsPerDegree;
            compassTargetMarker.anchoredPosition = new Vector2(markerLocalX, compassTargetMarker.anchoredPosition.y);
        }

        // 3. On-Screen Target Indicator Logic
        if (onScreenIndicator != null && cachedCamera != null)
        {
            Vector3 screenPos = cachedCamera.WorldToScreenPoint(targetPosition);

            // Target is in front of the camera if screenPos.z > 0
            bool isInFront = screenPos.z > 0f;
            bool isWithinScreen = screenPos.x >= 0f && screenPos.x <= Screen.width &&
                                  screenPos.y >= 0f && screenPos.y <= Screen.height;

            if (isInFront && isWithinScreen)
            {
                if (!onScreenIndicator.gameObject.activeSelf)
                {
                    onScreenIndicator.gameObject.SetActive(true);
                }
                // Update position
                onScreenIndicator.position = new Vector3(screenPos.x, screenPos.y, 0f);
            }
            else
            {
                if (onScreenIndicator.gameObject.activeSelf)
                {
                    onScreenIndicator.gameObject.SetActive(false);
                }
            }
        }
    }
}
