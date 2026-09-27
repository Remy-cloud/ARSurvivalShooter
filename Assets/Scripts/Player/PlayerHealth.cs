using System;
using UnityEngine;

// Player health. Lives on the Main Camera (the player is the phone).
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public bool IsDead => Current <= 0;

    public event Action<int, int> HealthChanged;   // current, max
    public event Action Damaged;
    public event Action Died;

    private void Awake() => ResetHealth();

    public void ResetHealth()
    {
        Current = maxHealth;
        HealthChanged?.Invoke(Current, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        Current = Mathf.Max(Current - amount, 0);
        HealthChanged?.Invoke(Current, maxHealth);
        Damaged?.Invoke();

        if (IsDead) Died?.Invoke();
    }

    [ContextMenu("Test: Take 10 Damage")]
    private void TestDamage() => TakeDamage(10);
}
