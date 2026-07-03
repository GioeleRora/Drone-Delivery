using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class WindArea : MonoBehaviour
{
    [Header("Wind Settings")]
    [Tooltip("Direzione e forza del vento in quest'area")]
    [SerializeField] private Vector3 windDirection = new Vector3(1f, 0f, 0f);
    [SerializeField] private float windStrength = 15f;
    [SerializeField] private float gustRandomness = 0.5f; // Variazione casuale della forza

    private List<Rigidbody> rigidbodiesInZone = new List<Rigidbody>();
    private Collider areaCollider;

    private void Awake()
    {
        areaCollider = GetComponent<Collider>();
        areaCollider.isTrigger = true; // Deve essere un trigger
    }

    private void FixedUpdate()
    {
        if (rigidbodiesInZone.Count == 0) return;

        // Calcoliamo la forza del vento con una componente casuale (raffiche)
        float currentStrength = windStrength * (1f + Random.Range(-gustRandomness, gustRandomness));
        Vector3 windForce = windDirection.normalized * currentStrength;

        // Applichiamo il vento a tutti i rigidbody nell'area
        foreach (Rigidbody rb in rigidbodiesInZone)
        {
            if (rb != null)
            {
                // Acceleration ignora la massa. Così il vento sposta il drone indipendentemente 
                // da eventuali futuri upgrade al peso della batteria, a meno che non compensiamo.
                // Se vogliamo che droni più pesanti resistano meglio al vento, usiamo ForceMode.Force
                rb.AddForce(windForce, ForceMode.Force);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rigidbodiesInZone.Contains(rb))
        {
            rigidbodiesInZone.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rigidbodiesInZone.Contains(rb))
        {
            rigidbodiesInZone.Remove(rb);
        }
    }

    // Disegna un gizmo nell'editor per visualizzare la direzione del vento
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, windDirection.normalized * 5f);
    }
}
