using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody))]
public class DragonController : MonoBehaviour
{
    [Header("Dragon Stats")]
    public string dragonName = "Dragon";
    public float maxHealth = 100f;
    public float movementSpeed = 5f;
    public float rotationSpeed = 10f;
    public float attackRange = 3f;
    
    [Header("Components")]
    public Animator animator;
    public Rigidbody rb;
    public Transform attackPoint;
    public GameObject fireParticlePrefab;
    public AudioSource audioSource;
    
    [Header("Audio Clips")]
    public AudioClip fireAttackSound;
    public AudioClip tailAttackSound;
    public AudioClip flyAttackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    
    [Header("Current State")]
    public float currentHealth;
    public bool isDead = false;
    public bool isFlying = false;
    public bool isAttacking = false;
    
    // Protected fields
    protected AbilitySystem abilitySystem;
    protected Vector3 movementDirection;
    protected Quaternion targetRotation;
    
    // Events
    public System.Action OnDeath;
    public System.Action<float> OnHealthChanged;
    
    protected virtual void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        abilitySystem = GetComponent<AbilitySystem>();
        if (abilitySystem == null)
            abilitySystem = gameObject.AddComponent<AbilitySystem>();
        
        currentHealth = maxHealth;
    }
    
    protected virtual void Start()
    {
        InitializeAbilities();
    }
    
    protected virtual void InitializeAbilities()
    {
        // This will be overridden in child classes
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        HandleAnimation();
    }
    
    protected virtual void FixedUpdate()
    {
        if (isDead) return;
        
        HandleMovement();
    }
    
    protected virtual void HandleMovement()
    {
        if (movementDirection != Vector3.zero)
        {
            // Move the dragon
            Vector3 moveVelocity = movementDirection * movementSpeed;
            if (isFlying)
                moveVelocity *= 1.5f; // Faster when flying
            
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
            
            // Rotate towards movement direction
            targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Slow down
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x * 0.9f,
                rb.linearVelocity.y,
                rb.linearVelocity.z * 0.9f
            );
        }
    }
    
    protected virtual void HandleAnimation()
    {
        // Update animator parameters
        float speed = rb.linearVelocity.magnitude / movementSpeed;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsFlying", isFlying);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }
    
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Trigger event
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        // Play hit animation
        animator.SetTrigger("Hit");
        
        // Play sound
        if (audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);
        
        // Camera shake
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController)
            cameraController.ShakeCamera(0.2f, 0.1f);
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
    
    protected virtual void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        
        if (audioSource && deathSound)
            audioSource.PlayOneShot(deathSound);
        
        // Disable collider and rigidbody
        Collider collider = GetComponent<Collider>();
        if (collider) collider.enabled = false;
        rb.isKinematic = true;
        
        // Notify game manager
        OnDeath?.Invoke();
    }
    
    public virtual void ResetDragon()
    {
        isDead = false;
        currentHealth = maxHealth;
        
        // Enable collider and rigidbody
        Collider collider = GetComponent<Collider>();
        if (collider) collider.enabled = true;
        rb.isKinematic = false;
        
        // Reset position (this would be set to spawn point in actual game)
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // Reset animation
        animator.Rebind();
        animator.Update(0f);
        
        OnHealthChanged?.Invoke(1f);
    }
    
    // Animation Events
    public void OnAttackStart()
    {
        isAttacking = true;
    }
    
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
    
    public void SpawnFireParticles()
    {
        if (fireParticlePrefab && attackPoint)
        {
            GameObject particles = Instantiate(fireParticlePrefab, attackPoint.position, attackPoint.rotation);
            Destroy(particles, 3f);
        }
    }
    
    public void PlayAttackSound(int attackType)
    {
        if (!audioSource) return;
        
        switch (attackType)
        {
            case 0: // Fire attack
                if (fireAttackSound) audioSource.PlayOneShot(fireAttackSound);
                break;
            case 1: // Tail attack
                if (tailAttackSound) audioSource.PlayOneShot(tailAttackSound);
                break;
            case 2: // Fly attack
                if (flyAttackSound) audioSource.PlayOneShot(flyAttackSound);
                break;
        }
    }
}