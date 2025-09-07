using UnityEngine; // <- THIS IS REQUIRED

public interface IDamageable
{
    void TakeDamage(int damage, Transform attacker = null);
}
