using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerDragonController : MonoBehaviour
{
    // ================= REFERENCES =================
    [Header("References")]
    public Animator animator;
    public Slider healthSlider;
    public Transform firePoint;               // Mouth / fire spawn point
    public ParticleSystem fireEffect;          // Fire VFX

    // ================= MOVEMENT =================
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    // ================= FLIGHT =================
    [Header("Flight")]
    public float flySpeed = 5f;
    public float gravity = -20f;
    public float maxFlyHeight = 8f;

    // ================= COMBAT =================
    [Header("Combat")]
    public int maxHealth = 100;
    public int fireDamage = 25;
    public float fireRange = 4f;
    public float fireCooldown = 1.5f;
    public LayerMask enemyLayer;

    // ================= PRIVATE =================
    private CharacterController controller;
    private Vector3 velocity;
    private bool isFlying;
    private bool isDead;
    private int currentHealth;
    private float fireTimer;

    // ================= START =================
    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;

        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (fireEffect)
            fireEffect.Stop();
    }

    // ================= UPDATE =================
    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleFlight();
        HandleFireAttack();
        UpdateAnimator();
    }

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0f, v);

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float speed = isFlying ? flySpeed : moveSpeed;
        controller.Move(move.normalized * speed * Time.deltaTime);
    }

    // ================= FLIGHT =================
    void HandleFlight()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isFlying = false;

        if (Input.GetKeyDown(KeyCode.Space))
            isFlying = true;

        if (isFlying)
        {
            velocity.y = flySpeed;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        if (transform.position.y >= maxFlyHeight)
            velocity.y = 0f;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isFlying = false;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // ================= FIRE ATTACK =================
    void HandleFireAttack()
    {
        fireTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && fireTimer <= 0f)
        {
            animator.SetTrigger("Fire");

            if (fireEffect)
                fireEffect.Play();

            DealFireDamage();

            fireTimer = fireCooldown;
        }
    }

    // ================= FIRE DAMAGE =================
    void DealFireDamage()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(
            firePoint.position,
            fireRange,
            enemyLayer
        );

        foreach (Collider enemy in hitEnemies)
        {
            enemy.SendMessage(
                "TakeDamage",
                fireDamage,
                SendMessageOptions.DontRequireReceiver
            );
        }
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
        isFlying = false;

        animator.SetBool("IsDead", true);
        animator.SetTrigger("Die");

        controller.enabled = false;
    }

    // ================= ANIMATOR =================
    void UpdateAnimator()
    {
        bool isMoving =
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsFlying", isFlying);
    }

    // ================= DEBUG =================
    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firePoint.position, fireRange);
    }
}
