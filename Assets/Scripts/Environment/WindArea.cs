using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindArea : MonoBehaviour
{
    [Header("Local Wind Settings")]
    [Tooltip("The direction of the local wind in world coordinates.")]
    [SerializeField] private Vector3 localWindDirection = Vector3.forward;
    [Tooltip("The base strength of the local wind.")]
    [SerializeField] private float localWindStrength = 10f;

    [Header("Local Oscillation (Gusts)")]
    [Tooltip("Enable local wind oscillations.")]
    [SerializeField] private bool useLocalOscillation = true;
    [Tooltip("Frequency of local wind gusts.")]
    [SerializeField] private float localGustFrequency = 1.2f;
    [Tooltip("Amplitude of local wind gusts.")]
    [SerializeField] private float localGustStrength = 4f;

    private void Awake()
    {
        // Make sure the collider is set as a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// Calculates the local wind force at a specific position.
    /// </summary>
    public Vector3 GetLocalWind(Vector3 position)
    {
        Vector3 baseLocalWind = localWindDirection.normalized * localWindStrength;

        if (useLocalOscillation)
        {
            float time = Time.time * localGustFrequency;
            float gust = Mathf.Sin(time) * localGustStrength;
            baseLocalWind += localWindDirection.normalized * gust;
        }

        return baseLocalWind;
    }

    private void OnTriggerEnter(Collider other)
    {
        WindReceiver receiver = other.GetComponentInParent<WindReceiver>();
        if (receiver != null)
        {
            receiver.RegisterWindArea(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WindReceiver receiver = other.GetComponentInParent<WindReceiver>();
        if (receiver != null)
        {
            receiver.UnregisterWindArea(this);
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // Semi-transparent blue for the wind zone volume
        Gizmos.color = new Color(0f, 0.6f, 0.9f, 0.15f);
        
        // Draw matching primitive trigger volume
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 0.6f, 0.9f, 0.4f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = new Color(0f, 0.6f, 0.9f, 0.4f);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        // Draw wind direction arrow
        Gizmos.matrix = Matrix4x4.identity; // back to world coordinates
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 dir = localWindDirection.normalized * (localWindStrength * 0.5f);
        
        if (dir.magnitude > 0.1f)
        {
            Gizmos.DrawRay(start, dir);
            
            // Draw arrowhead
            Quaternion rot = Quaternion.LookRotation(dir);
            Vector3 right = rot * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
            Vector3 left = rot * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;
            Gizmos.DrawRay(start + dir, right * 1f);
            Gizmos.DrawRay(start + dir, left * 1f);
        }
    }
}
