/// <summary>
/// Anything that can be hurt by a projectile or a melee attack
/// (enemies, the player). Lets projectiles damage objects without
/// knowing their exact class (abstraction + polymorphism).
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
    bool IsDead { get; }
}
