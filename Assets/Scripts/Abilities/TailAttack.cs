using UnityEngine;

public class TailAttack : AbilityBase
{
    [Header("Tail Attack Settings")]
    public float damage = 25f;
    public float knockbackForce = 10f;
    public float attackArc = 120f;
    public float attackDistance = 4f;
    
    public override bool Use()
    {
        if (!base.Use()) return false;
        
        // Perform tail swipe
        PerformTailSwipe();
        
        EndAbility();
        return true;
    }
    
    void PerformTailSwipe()
    {
        // Get all dragons in range
        DragonController[] dragons = FindObjectsOfType<DragonController>();
        
        foreach (DragonController dragon in dragons)
        {
            if (dragon == owner || dragon.isDead) continue;
            
            Vector3 directionToEnemy = (dragon.transform.position - owner.transform.position);
            float distance = directionToEnemy.magnitude;
            
            if (distance <= attackDistance)
            {
                // Check if enemy is within attack arc
                float angle = Vector3.Angle(owner.transform.forward, directionToEnemy.normalized);
                
                if (angle <= attackArc / 2f)
                {
                    // Deal damage
                    dragon.TakeDamage(damage);
                    
                    // Apply knockback
                    ApplyKnockback(dragon, directionToEnemy.normalized);
                }
            }
        }
    }
    
    void ApplyKnockback(DragonController enemy, Vector3 direction)
    {
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb)
        {
            enemyRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}