using UnityEngine;

public class FireAttack : AbilityBase
{
    [Header("Fire Attack Settings")]
    public float damage = 20f;
    public float damageOverTime = 5f;
    public float dotDuration = 3f;
    public float fireRadius = 3f;
    public LayerMask enemyLayer;
    
    private ParticleSystem fireParticles;
    private Collider[] hitEnemies = new Collider[10];
    
    public override bool Use()
    {
        if (!base.Use()) return false;
        
        // Create fire cone
        CreateFireCone();
        
        // Damage enemies in cone
        DamageEnemiesInCone();
        
        EndAbility();
        return true;
    }
    
    void CreateFireCone()
    {
        if (owner.attackPoint)
        {
            // Instantiate fire particles
            if (owner.fireParticlePrefab)
            {
                GameObject fireObj = Instantiate(owner.fireParticlePrefab, 
                    owner.attackPoint.position, 
                    owner.attackPoint.rotation);
                
                fireParticles = fireObj.GetComponent<ParticleSystem>();
                if (fireParticles)
                {
                    fireParticles.Play();
                    Destroy(fireObj, 2f);
                }
            }
        }
    }
    
    void DamageEnemiesInCone()
    {
        int numHits = Physics.OverlapSphereNonAlloc(
            owner.attackPoint.position, 
            fireRadius, 
            hitEnemies, 
            enemyLayer
        );
        
        for (int i = 0; i < numHits; i++)
        {
            DragonController enemy = hitEnemies[i].GetComponent<DragonController>();
            if (enemy != null && enemy != owner)
            {
                // Calculate direction to enemy
                Vector3 directionToEnemy = (enemy.transform.position - owner.transform.position).normalized;
                float dot = Vector3.Dot(owner.transform.forward, directionToEnemy);
                
                // Only damage enemies in front (120 degree cone)
                if (dot > 0.5f)
                {
                    enemy.TakeDamage(damage);
                    
                    // Apply damage over time
                    ApplyDamageOverTime(enemy);
                }
            }
        }
    }
    
    void ApplyDamageOverTime(DragonController enemy)
    {
        // In a real implementation, you would create a DamageOverTime component
        // or use a coroutine to apply damage over time
        enemy.StartCoroutine(DamageOverTimeCoroutine(enemy));
    }
    
    System.Collections.IEnumerator DamageOverTimeCoroutine(DragonController enemy)
    {
        float elapsed = 0f;
        float interval = 0.5f;
        
        while (elapsed < dotDuration && enemy != null && !enemy.isDead)
        {
            enemy.TakeDamage(damageOverTime * interval);
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}