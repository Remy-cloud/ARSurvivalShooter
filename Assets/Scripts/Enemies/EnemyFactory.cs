using UnityEngine;

public enum EnemyType { Melee, Shooter }

// Factory pattern: the only place that knows which prefab makes which enemy.
public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private EnemyBase meleePrefab;
    [SerializeField] private EnemyBase shooterPrefab;
    [SerializeField] private ProjectilePool enemyBulletPool;

    public bool HasShooter => shooterPrefab != null;

    public EnemyBase Create(EnemyType type, Vector3 position, Quaternion rotation)
    {
        EnemyBase prefab = type == EnemyType.Shooter && HasShooter ? shooterPrefab : meleePrefab;
        EnemyBase enemy = Instantiate(prefab, position, rotation);
        if (enemy is ShooterEnemy shooter) shooter.SetPool(enemyBulletPool);
        return enemy;
    }
}
