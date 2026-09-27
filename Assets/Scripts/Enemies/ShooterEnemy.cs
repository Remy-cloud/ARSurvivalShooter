using System;
using UnityEngine;

// Walks toward the player, stops at range and shoots pooled bullets at them.
public class ShooterEnemy : EnemyBase
{
    [Header("Shooter")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireCooldown = 1.8f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Walk Sway")]
    [SerializeField] private Transform model;
    [SerializeField] private float swayAngle = 6f;
    [SerializeField] private float swaySpeed = 10f;

    public static event Action Shot;

    private ProjectilePool pool;
    private float nextShotTime;
    private Vector3 lastPosition;
    private Quaternion modelRestRotation;

    // Given by the EnemyFactory when this enemy is created.
    public void SetPool(ProjectilePool bulletPool) => pool = bulletPool;

    protected override void OnInit()
    {
        nextShotTime = Time.time + 1f;
        lastPosition = transform.position;
        if (model) modelRestRotation = model.localRotation;
    }

    protected override void Update()
    {
        base.Update();
        Sway();
    }

    protected override void Attack()
    {
        if (Time.time < nextShotTime || pool == null) return;
        nextShotTime = Time.time + fireCooldown;

        Vector3 direction = (player.position - muzzle.position).normalized;
        if (pool.Fire(muzzle.position, direction, damage, hitMask))
            Shot?.Invoke();
    }

    // Small side-to-side tilt while moving, so the static model looks like it walks.
    private void Sway()
    {
        if (!model || IsDead) return;
        float speed = (transform.position - lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = transform.position;

        float amount = Mathf.Clamp01(speed / moveSpeed);
        float tilt = Mathf.Sin(Time.time * swaySpeed) * swayAngle * amount;
        model.localRotation = modelRestRotation * Quaternion.Euler(0f, 0f, tilt);
    }
}
