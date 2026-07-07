using UnityEngine;
using System;

[RequireComponent(typeof(DroneMovement))]
public class DroneHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points of the drone")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Collision Damage Settings")]
    [Tooltip("Velocity threshold in m/s above which collision damage is applied")]
    [SerializeField] private float damageThreshold = 3.0f;

    [Tooltip("Damage multiplier per m/s over the threshold (HP per m/s)")]
    [SerializeField] private float damageMultiplier = 10f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth <= 0f;

    // Events
    public event Action<float, float> OnHealthChanged;
    public event Action OnDroneDestroyed;

    private DroneMovement droneMovement;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        droneMovement = GetComponent<DroneMovement>();
    }

    private void Start()
    {
        // Initial call to update listeners with initial health values
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsDead) return;

        // Calculate damage based on relative velocity
        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;
        if (relativeVelocityMagnitude > damageThreshold)
        {
            float excessVelocity = relativeVelocityMagnitude - damageThreshold;
            float damage = excessVelocity * damageMultiplier;
            TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDroneDestroyed?.Invoke();
        if (droneMovement != null)
        {
            droneMovement.SetMotorsState(false);
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
