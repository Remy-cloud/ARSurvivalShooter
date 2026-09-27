using System;
using UnityEngine;

// Walks up to the player and bites when close, with a cooldown.
public class MeleeEnemy : EnemyBase
{
    [Header("Melee")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private Animator animator;
    [SerializeField] private string attackState = "Attack";

    public static event Action MeleeAttacked;

    private PlayerHealth playerHealth;
    private float nextAttackTime;

    protected override void OnInit()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        nextAttackTime = Time.time + 0.5f;
    }

    protected override void Attack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        if (animator) animator.Play(attackState, 0, 0f);
        if (playerHealth) playerHealth.TakeDamage(damage);
        MeleeAttacked?.Invoke();
    }
}
