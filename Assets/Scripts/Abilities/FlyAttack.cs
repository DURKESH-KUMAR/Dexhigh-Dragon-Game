using UnityEngine;

public class FlyAttack : AbilityBase
{
    [Header("Fly Attack Settings")]
    public float flyDuration = 5f;
    public float diveDamage = 30f;
    public float diveRadius = 4f;
    public float flightHeight = 10f;
    
    private bool isDiving = false;
    private Vector3 diveTarget;
    
    public override bool Use()
    {
        if (!base.Use()) return false;
        
        // Start flying
        StartFlying();
        
        return true;
    }
    
    protected override void OnAbilityActive()
    {
        base.OnAbilityActive();
        
        if (isDiving)
        {
            HandleDive();
        }
        else if (activeTime >= flyDuration - 1f)
        {
            // Time to dive
            PrepareDive();
        }
    }
    
    void StartFlying()
    {
        owner.isFlying = true;
        
        // Move dragon up
        Vector3 flyPosition = owner.transform.position;
        flyPosition.y = flightHeight;
        owner.transform.position = flyPosition;
        
        // Disable ground collision temporarily
        owner.rb.useGravity = false;
    }
    
    void PrepareDive()
    {
        // Find target for dive
        DragonController target = FindNearestEnemy();
        if (target != null)
        {
            diveTarget = target.transform.position;
            isDiving = true;
            
            // Play dive animation
            owner.animator.SetTrigger("Dive");
        }
    }
    
    void HandleDive()
    {
        // Move towards dive target
        Vector3 diveDirection = (diveTarget - owner.transform.position).normalized;
        owner.rb.linearVelocity = diveDirection * 20f;
        
        // Check if reached ground
        if (owner.transform.position.y <= 1f)
        {
            ExecuteDiveImpact();
        }
    }
    
    void ExecuteDiveImpact()
    {
        // Create impact effect
        CreateImpactEffect();
        
        // Damage enemies in radius
        Collider[] hitEnemies = Physics.OverlapSphere(owner.transform.position, diveRadius);
        foreach (Collider collider in hitEnemies)
        {
            DragonController enemy = collider.GetComponent<DragonController>();
            if (enemy != null && enemy != owner)
            {
                enemy.TakeDamage(diveDamage);
            }
        }
        
        // Land
        owner.isFlying = false;
        owner.rb.useGravity = true;
        isDiving = false;
        
        EndAbility();
    }
    
    void CreateImpactEffect()
    {
        // Create dust/impact particles at landing point
        // This would be implemented with particle systems
    }
}