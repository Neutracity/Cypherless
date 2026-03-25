namespace Cypherless;

public interface IDamagable
{
    public int MaxHealth { get; set; }
    public int Health { get; set; }

    public void TakeDamage(int damage = 1)
    {
        Health -= damage;
    }
}