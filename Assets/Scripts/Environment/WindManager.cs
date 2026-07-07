using UnityEngine;

[DisallowMultipleComponent]
public class WindManager : MonoBehaviour
{
    public static WindManager Instance { get; private set; }

    [Header("Base Wind Settings")]
    [Tooltip("The direction of the steady global wind.")]
    [SerializeField] private Vector3 baseWindDirection = new Vector3(1f, 0f, 0f);
    [Tooltip("The base strength of the steady global wind.")]
    [SerializeField] private float baseWindStrength = 5f;

    [Header("Oscillation Settings (Gusts)")]
    [Tooltip("Frequency of wind gust oscillations (Hz/Speed).")]
    [SerializeField] private float gustFrequency = 0.5f;
    [Tooltip("Amplitude of the wind gust oscillations.")]
    [SerializeField] private float gustStrength = 3f;

    [Header("Altitude Scaling")]
    [Tooltip("The maximum multiplier applied to wind at critical altitude.")]
    [SerializeField] private float altitudeScaleFactor = 2f;
    [Tooltip("Exponent for altitude scaling. 1 = Linear, 2 = Exponential (Quadratic)")]
    [SerializeField] private float altitudeScaleExponent = 1.5f;
    [Tooltip("Altitude at which the wind reaches maximum strength.")]
    [SerializeField] private float criticalAltitude = 65f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Calculates the global wind force vector at a given world position.
    /// </summary>
    /// <param name="position">The position to check wind force at (typically the drone's position).</param>
    /// <returns>The calculated wind force vector.</returns>
    public Vector3 GetGlobalWind(Vector3 position)
    {
        // 1. Base wind vector
        Vector3 baseWind = baseWindDirection.normalized * baseWindStrength;

        // 2. Gust oscillation (sine/cosine waves to simulate dynamic, multi-dimensional gusts)
        float time = Time.time * gustFrequency;
        float gustX = Mathf.Sin(time) * gustStrength;
        float gustZ = Mathf.Cos(time * 0.7f) * gustStrength; // Slightly different frequency for Z to make it look organic
        Vector3 gustWind = new Vector3(gustX, 0f, gustZ);

        // Raw global wind
        Vector3 rawWind = baseWind + gustWind;

        // 3. Altitude scaling with safety check for criticalAltitude division
        float height = Mathf.Max(0f, position.y);
        float heightRatio = criticalAltitude > 0.001f ? Mathf.Clamp01(height / criticalAltitude) : 0f;
        float scaleMultiplier = Mathf.Pow(heightRatio, altitudeScaleExponent) * altitudeScaleFactor;

        return rawWind * scaleMultiplier;
    }
}
