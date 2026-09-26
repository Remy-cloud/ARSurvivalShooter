using UnityEngine;

/// <summary>
/// A pooled bullet. It is never Instantiated or Destroyed during gameplay:
/// the ProjectilePool hands it out with Launch(...) and takes it back with Release().
/// Movement uses a raycast each frame so small, fast bullets never pass through targets.
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;        // metres per second
    [SerializeField] private float lifetime = 2f;     // seconds before it returns to the pool
    [SerializeField] private float radius = 0.02f;    // hit radius (sphere cast)

    private ProjectilePool pool;       // the pool that owns this bullet
    private Vector3 direction;
    private int damage;
    private LayerMask hitMask;
    private float timeLeft;
    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    /// <summary>Called once by the pool when the bullet is first created.</summary>
    public void SetPool(ProjectilePool owner) => pool = owner;

    /// <summary>Resets every value and fires the bullet (proper reset of a pooled object).</summary>
    public void Launch(Vector3 position, Vector3 dir, int dmg, LayerMask mask)
    {
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(dir));
        direction = dir.normalized;
        damage = dmg;
        hitMask = mask;
        timeLeft = lifetime;
        if (trail) trail.Clear();      // remove the old trail from its previous flight
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;

        // Check what we would hit between this frame and the next.
        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, step, hitMask, QueryTriggerInteraction.Collide))
        {
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null && !target.IsDead) target.TakeDamage(damage);
            Release();
            return;
        }

        transform.position += direction * step;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f) Release();
    }

    /// <summary>Hide the bullet and give it back to the pool for reuse.</summary>
    public void Release()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        pool.Return(this);
    }
}
