using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDragonAI : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform attackPoint;
    public Slider healthSlider;
    public Collider bodyCollider;

    [Header("Ranges")]
    public float detectionRange = 20f;
    public float attackRange = 3f;

    [Header("Attack")]
    public int attackDamage = 20;
    public float attackCooldown = 2f;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;

    [Header("Rotation")]
    public float rotationSpeed = 7f;

    [Header("Health")]
    public int maxHealth = 100;

    [Header("Death")]
    public float destroyDelay = 6f;

    private NavMeshAgent agent;
    private Transform player;
    private float attackTimer;
    private int currentHealth;
    private bool isDead;

    // ================= START =================
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange;
        agent.updateRotation = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
        attackTimer = attackCooldown;

        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // ================= UPDATE =================
    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // OUTSIDE DETECTION
        if (distance > detectionRange)
        {
            StopMovement();
            return;
        }

        // CHASE
        if (distance > attackRange)
        {
            ChasePlayer();
        }
        // ATTACK (maintains distance)
        else
        {
            StopMovement();
            FacePlayer();
            TryAttack();
        }
    }

    // ================= CHASE =================
    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetBool("IsMoving", true);

        RotateTowardsMovement();
    }

    // ================= STOP =================
    void StopMovement()
    {
        agent.isStopped = true;
        animator.SetBool("IsMoving", false);
    }

    // ================= ATTACK =================
    void TryAttack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        animator.SetTrigger("Attack");

        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRadius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            hit.SendMessage(
                "TakeDamage",
                attackDamage,
                SendMessageOptions.DontRequireReceiver
            );
        }

        attackTimer = attackCooldown;
    }

    // ================= ROTATION =================
    void RotateTowardsMovement()
    {
        Vector3 vel = agent.velocity;
        vel.y = 0f;

        if (vel.sqrMagnitude < 0.1f) return;

        Quaternion targetRot = Quaternion.LookRotation(vel);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRot,
            Time.deltaTime * rotationSpeed
        );
    }

    // ================= DAMAGE =================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (healthSlider)
            healthSlider.value = currentHealth;

        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            Die();
    }

    // ================= DEATH =================
    void Die()
    {
        isDead = true;

        agent.isStopped = true;
        agent.enabled = false;

        if (bodyCollider)
            bodyCollider.enabled = false;

        animator.SetBool("IsDead", true);
        animator.SetTrigger("Die");

        Destroy(gameObject, destroyDelay);
    }

    // ================= DEBUG =================
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
