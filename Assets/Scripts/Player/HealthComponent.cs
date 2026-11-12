using UnityEngine;
using UnityEngine.Rendering;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    private int currHealth;
    [SerializeField]
    private int maxHealth;

    /// <summary>
    /// Takes damage from the specified amount.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    public void TakeDamage(int damage)
    {
        currHealth -= damage;
        currHealth = Mathf.Clamp(currHealth, 0, maxHealth);
        if (currHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to heal by.</param>
    public void Heal(int amount)
    {
        currHealth += amount;
        currHealth = Mathf.Clamp(currHealth, 0, maxHealth);
    }

    /// <summary>
    /// Calls the object's death function
    /// </summary>
    private void Die()
    {
        // Implement death logic here
    }
}
