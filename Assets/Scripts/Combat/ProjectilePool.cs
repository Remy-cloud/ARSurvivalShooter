using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OBJECT POOL pattern for projectiles.
/// - Pre-initialised: all bullets are created once in Awake (before gameplay).
/// - Reusable: Get() takes an inactive bullet, Return() puts it back.
/// - No Instantiate/Destroy during gameplay.
/// One pool for the player's bullets, another for the shooter enemies' bullets.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int poolSize = 30;

    private readonly Queue<Projectile> available = new Queue<Projectile>();

    public int AvailableCount => available.Count;   // handy for debugging / documentation

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            Projectile p = Instantiate(projectilePrefab, transform);   // only happens here, at load time
            p.SetPool(this);
            p.gameObject.SetActive(false);
            available.Enqueue(p);
        }
    }

    /// <summary>Fire a bullet from the pool. Returns false if every bullet is already in the air.</summary>
    public bool Fire(Vector3 position, Vector3 direction, int damage, LayerMask hitMask)
    {
        if (available.Count == 0) return false;     // pool exhausted: skip this shot instead of creating a new one
        Projectile p = available.Dequeue();
        p.Launch(position, direction, damage, hitMask);
        return true;
    }

    /// <summary>Called by a Projectile when it hits something or its lifetime ends.</summary>
    public void Return(Projectile p)
    {
        available.Enqueue(p);
    }

    /// <summary>Send every active bullet back (used when the game ends).</summary>
    public void ReturnAll()
    {
        foreach (Transform child in transform)
        {
            Projectile p = child.GetComponent<Projectile>();
            if (p != null && p.gameObject.activeSelf) p.Release();
        }
    }
}
