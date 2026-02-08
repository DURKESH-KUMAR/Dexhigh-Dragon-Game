using UnityEngine;
using System.Collections;

public class PlayerDragon : DragonController
{
    [Header("Player Input")]
    public KeyCode fireAttackKey = KeyCode.Q;
    public KeyCode tailAttackKey = KeyCode.E;
    public KeyCode flyAttackKey = KeyCode.R;
    public KeyCode jumpKey = KeyCode.Space;
    
    [Header("Abilities")]
    public FireAttack fireAttack;
    public TailAttack tailAttack;
    public FlyAttack flyAttack;
    
    [Header("Camera Reference")]
    public Camera playerCamera; // ADD THIS
    
    private bool canJump = true;
    private float jumpCooldown = 1f;
    private float lastJumpTime;
    private Vector3 cameraForward;
    private Vector3 cameraRight;
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize camera reference
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("No main camera found! Please assign a camera.");
                // Create a backup camera
                playerCamera = FindObjectOfType<Camera>();
                if (playerCamera == null)
                {
                    GameObject camObj = new GameObject("BackupCamera");
                    playerCamera = camObj.AddComponent<Camera>();
                    camObj.AddComponent<AudioListener>();
                    camObj.tag = "MainCamera";
                }
            }
        }
        
        // Subscribe to events
        OnHealthChanged += (healthPercentage) => {
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateHealthBar(true, currentHealth, maxHealth);
        };
        
        OnDeath += () => {
            if (GameManager.Instance != null)
                GameManager.Instance.DragonDied(this);
        };
        
        // Initialize camera vectors
        UpdateCameraVectors();
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Update camera vectors every frame in case camera moves
        UpdateCameraVectors();
        
        HandleInput();
        UpdateAbilityCooldowns();
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // Handle jumping
        if (canJump && Input.GetKey(jumpKey) && Time.time > lastJumpTime + jumpCooldown)
        {
            Jump();
        }
    }
    
    // NEW METHOD: Update camera direction vectors
    void UpdateCameraVectors()
    {
        if (playerCamera != null)
        {
            cameraForward = playerCamera.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();
            
            cameraRight = playerCamera.transform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();
        }
        else
        {
            // Fallback to world directions
            cameraForward = Vector3.forward;
            cameraRight = Vector3.right;
        }
    }
    
    void HandleInput()
    {
        // SAFE CHECK: Ensure we have camera references
        if (cameraForward == Vector3.zero || cameraRight == Vector3.zero)
        {
            UpdateCameraVectors();
        }
        
        // Movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculate movement direction relative to camera
        if (horizontal != 0 || vertical != 0)
        {
            movementDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        }
        else
        {
            movementDirection = Vector3.zero;
        }
        
        // Ability input - WITH NULL CHECKS
        if (Input.GetKeyDown(fireAttackKey))
        {
            UseFireAttack();
        }
        
        if (Input.GetKeyDown(tailAttackKey))
        {
            UseTailAttack();
        }
        
        if (Input.GetKeyDown(flyAttackKey))
        {
            UseFlyAttack();
        }
    }
    
    void UpdateAbilityCooldowns()
    {
        if (abilitySystem == null) 
        {
            Debug.LogWarning("AbilitySystem is null in PlayerDragon");
            return;
        }
        
        for (int i = 0; i < abilitySystem.abilities.Length; i++)
        {
            if (i < abilitySystem.abilities.Length && UIManager.Instance != null)
            {
                float cooldownProgress = abilitySystem.GetCooldownProgress(i);
                bool isReady = abilitySystem.IsAbilityReady(i);
                UIManager.Instance.UpdateAbilityUI(i, cooldownProgress, isReady);
            }
        }
    }
    
    void Jump()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody is null!");
            return;
        }
        
        rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);
        
        if (animator != null)
            animator.SetTrigger("Jump");
            
        lastJumpTime = Time.time;
        
        // Start jump cooldown
        StartCoroutine(JumpCooldown());
    }
    
    System.Collections.IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
    
    void UseFireAttack()
    {
        if (abilitySystem == null)
        {
            Debug.LogError("AbilitySystem is null!");
            return;
        }
        
        if (abilitySystem.UseAbility(0))
        {
            if (animator != null)
                animator.SetTrigger("FireAttack");
        }
    }
    
    void UseTailAttack()
    {
        if (abilitySystem == null)
        {
            Debug.LogError("AbilitySystem is null!");
            return;
        }
        
        if (abilitySystem.UseAbility(1))
        {
            if (animator != null)
                animator.SetTrigger("TailAttack");
        }
    }
    
    void UseFlyAttack()
    {
        if (abilitySystem == null)
        {
            Debug.LogError("AbilitySystem is null!");
            return;
        }
        
        if (abilitySystem.UseAbility(2))
        {
            if (animator != null)
                animator.SetTrigger("FlyAttack");
                
            isFlying = true;
            
            // End flying after duration
            if (flyAttack != null)
                StartCoroutine(EndFlying(flyAttack.flyDuration));
            else
                StartCoroutine(EndFlying(5f)); // Default duration
        }
    }
    
    System.Collections.IEnumerator EndFlying(float duration)
    {
        yield return new WaitForSeconds(duration);
        isFlying = false;
        
        if (animator != null)
            animator.SetTrigger("Land");
    }
    
    protected override void InitializeAbilities()
    {
        // Create abilities if they don't exist - WITH NULL CHECKS
        if (fireAttack == null)
        {
            fireAttack = gameObject.GetComponent<FireAttack>();
            if (fireAttack == null)
                fireAttack = gameObject.AddComponent<FireAttack>();
                
            if (fireAttack != null)
                fireAttack.Initialize(this);
        }
        
        if (tailAttack == null)
        {
            tailAttack = gameObject.GetComponent<TailAttack>();
            if (tailAttack == null)
                tailAttack = gameObject.AddComponent<TailAttack>();
                
            if (tailAttack != null)
                tailAttack.Initialize(this);
        }
        
        if (flyAttack == null)
        {
            flyAttack = gameObject.GetComponent<FlyAttack>();
            if (flyAttack == null)
                flyAttack = gameObject.AddComponent<FlyAttack>();
                
            if (flyAttack != null)
                flyAttack.Initialize(this);
        }
        
        // Initialize ability system - WITH NULL CHECK
        if (abilitySystem != null)
        {
            abilitySystem.Initialize(new AbilityBase[] { fireAttack, tailAttack, flyAttack });
        }
        else
        {
            Debug.LogError("AbilitySystem is null in PlayerDragon!");
        }
    }
    
    public override void ResetDragon()
    {
        base.ResetDragon();
        
        // Reset flying state
        isFlying = false;
        
        // Reset abilities
        if (abilitySystem != null)
        {
            abilitySystem.ResetAllAbilities();
        }
    }
}