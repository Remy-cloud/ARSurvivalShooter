using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shared enemy logic: health, moving to the player, hit feedback, death.
// Subclasses only decide how to attack.
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected int maxHealth = 2;
    [SerializeField] protected float moveSpeed = 0.25f;
    [SerializeField] protected float attackRange = 0.35f;
    [SerializeField] private int scoreValue = 10;

    [Header("Hit Feedback")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] private float hitFlashTime = 0.1f;

    [Header("Spreading")]
    [SerializeField] private float separationRadius = 0.2f;
    [SerializeField] private float separationStrength = 1.5f;
    [SerializeField] private float approachSpread = 60f;   // degrees either side of the spawn direction

    public static event Action<EnemyBase> AnySpawned;
    public static event Action<EnemyBase> AnyHit;
    public static event Action<EnemyBase> AnyKilled;
    public event Action<EnemyBase> Removed;

    protected Transform player;
    private int health;
    private MaterialPropertyBlock block;
    private Vector3 baseScale;
    private float approachAngle;

    private static readonly List<EnemyBase> active = new List<EnemyBase>();

    public bool IsDead => health <= 0;
    public int ScoreValue => scoreValue;

    public void Init(Transform playerTarget)
    {
        player = playerTarget;
        health = maxHealth;
        baseScale = transform.localScale;
        block = new MaterialPropertyBlock();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>();

        // Each enemy aims for its own spot around the player, roughly on its side.
        Vector3 fromPlayer = transform.position - PlayerFloorPosition();
        approachAngle = Mathf.Atan2(fromPlayer.x, fromPlayer.z) * Mathf.Rad2Deg
                        + UnityEngine.Random.Range(-approachSpread, approachSpread);
        active.Add(this);

        OnInit();
        AnySpawned?.Invoke(this);
    }

    protected virtual void OnInit() { }
    protected abstract void Attack();

    protected virtual void Update()
    {
        if (IsDead || player == null) return;

        Vector3 playerPos = PlayerFloorPosition();
        Vector3 toPlayer = playerPos - transform.position;
        Vector3 spot = playerPos + Quaternion.Euler(0f, approachAngle, 0f) * Vector3.forward * (attackRange * 0.9f);
        Vector3 toSpot = spot - transform.position;

        if (toPlayer.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer), 8f * Time.deltaTime);

        Vector3 move = Separation() * separationStrength;
        bool inRange = toPlayer.magnitude <= attackRange;
        if (!inRange && toSpot.sqrMagnitude > 0.0004f) move += toSpot.normalized;

        transform.position += Vector3.ClampMagnitude(move, 1f) * moveSpeed * Time.deltaTime;

        if (inRange) Attack();
    }

    // Pushes away from nearby enemies so they don't stack.
    private Vector3 Separation()
    {
        Vector3 push = Vector3.zero;
        foreach (EnemyBase other in active)
        {
            if (other == this || other.IsDead) continue;
            Vector3 away = transform.position - other.transform.position;
            away.y = 0f;
            float d = away.magnitude;
            if (d > 0.0001f && d < separationRadius)
                push += away / d * (1f - d / separationRadius);
        }
        return push;
    }

    // The player's position dropped to this enemy's floor height.
    protected Vector3 PlayerFloorPosition() =>
        new Vector3(player.position.x, transform.position.y, player.position.z);

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        health -= amount;
        AnyHit?.Invoke(this);

        if (IsDead) StartCoroutine(Die());
        else StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        SetTint(hitColor);
        transform.localScale = baseScale * 1.15f;
        yield return new WaitForSeconds(hitFlashTime);
        SetTint(Color.white);
        transform.localScale = baseScale;
    }

    private IEnumerator Die()
    {
        AnyKilled?.Invoke(this);
        SetTint(hitColor);
        for (float t = 0; t < 0.25f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(baseScale, Vector3.zero, t / 0.25f);
            yield return null;
        }
        Remove();
    }

    // Removes the enemy without giving score (used when the game ends).
    public void Remove()
    {
        active.Remove(this);
        Removed?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnDestroy() => active.Remove(this);

    private void SetTint(Color c)
    {
        foreach (Renderer r in renderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", c);
            r.SetPropertyBlock(block);
        }
    }
}
