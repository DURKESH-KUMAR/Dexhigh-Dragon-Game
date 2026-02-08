using UnityEngine;

public class PlayerDragonAttack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float fireDamage = 20f;
    public float tailDamage = 15f;
    public float flyDamage = 25f;

    private float currentDamage;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            FireAttack();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            TailAttack();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            FlyAttack();
    }

    void FireAttack()
    {
        currentDamage = fireDamage;
        animator.SetTrigger("FireAttack");
    }

    void TailAttack()
    {
        currentDamage = tailDamage;
        animator.SetTrigger("TailAttack");
    }

    void FlyAttack()
    {
        currentDamage = flyDamage;
        animator.SetTrigger("FlyAttack");
    }

    // 🔥 CALLED FROM ANIMATION EVENT
    public void ApplyDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            DragonHealth health = hit.GetComponentInParent<DragonHealth>();
            if (health != null)
            {
                health.TakeDamage(currentDamage);
            }
        }
    }

    // Debug helper
    void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
