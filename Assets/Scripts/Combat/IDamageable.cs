/// <summary>
//Anything that can be hurt by a projectile or a melee attack
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
    bool IsDead { get; }
}
