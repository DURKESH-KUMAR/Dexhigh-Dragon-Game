using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))] // ADD THIS
public class AIDragon : DragonController
{
    [Header("AI Settings")]
    public float detectionRange = 15f;
    public float attackCooldown = 2f;
    public float decisionInterval = 1f;
    public float wanderRadius = 10f;
    
    [Header("Behavior Weights")]
    public float attackWeight = 0.6f;
    public float evadeWeight = 0.3f;
    public float wanderWeight = 0.1f;
    
    [Header("Abilities")]
    public FireAttack fireAttack;
    public TailAttack tailAttack;
    public FlyAttack flyAttack;
    
    [Header("References")]
    public Transform playerTarget; // ADD MANUAL REFERENCE
    
    // Private variables
    private NavMeshAgent navMeshAgent;
    private float lastAttackTime;
    private float lastDecisionTime;
    private Vector3 wanderPoint;
    private AIState currentState = AIState.Idle;
    private bool hasValidTarget = false;
    
    // AI States
    private enum AIState
    {
        Idle,
        Chase,
        Attack,
        Evade,
        Wander
    }
    
    protected override void Start()
    {
        base.Start();
        
        InitializeAI();
    }
    
    void InitializeAI()
    {
        // Get or add NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogWarning("NavMeshAgent not found on AIDragon. Adding one.");
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // Configure NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = movementSpeed;
            navMeshAgent.angularSpeed = rotationSpeed * 50f;
            navMeshAgent.acceleration = 8f;
            navMeshAgent.stoppingDistance = attackRange * 0.8f;
            navMeshAgent.height = 4f;
            navMeshAgent.radius = 1f;
        }
        
        // Get player reference - TRY MULTIPLE METHODS
        if (playerTarget == null)
        {
            // Method 1: Try to get from GameManager
            if (GameManager.Instance != null && GameManager.Instance.playerDragon != null)
            {
                playerTarget = GameManager.Instance.playerDragon.transform;
            }
            // Method 2: Find by tag
            else if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
            }
            // Method 3: Find by type
            else
            {
                // // PlayerDragon player = FindObjectOfType<PlayerDragon>();
                // if (player != null)
                //     playerTarget = player.transform;
            }
        }
        
        // Validate player reference
        hasValidTarget = (playerTarget != null);
        if (!hasValidTarget)
        {
            Debug.LogError("AIDragon: Could not find player target! AI will not function.");
            
            // Create a dummy target for testing
            CreateDummyTarget();
        }
        
        // Subscribe to events WITH NULL CHECKS
        OnHealthChanged += (healthPercentage) => {
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateHealthBar(false, currentHealth, maxHealth);
        };
        
        OnDeath += () => {
            if (GameManager.Instance != null)
                GameManager.Instance.DragonDied(this);
        };
        
        // Initial wander point
        wanderPoint = GetWanderPoint();
        lastDecisionTime = Time.time - decisionInterval; // Allow immediate decision
        
        Debug.Log($"AIDragon initialized. Has target: {hasValidTarget}, Has NavMesh: {navMeshAgent != null}");
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Only make decisions if we have a valid target
        if (hasValidTarget)
        {
            MakeDecision();
            UpdateState();
        }
        else
        {
            // Fallback behavior when no target
            WanderBehavior();
        }
    }
    
    void MakeDecision()
    {
        // SAFE CHECK: Ensure we have required references
        if (playerTarget == null || !hasValidTarget)
        {
            // Try to reacquire target
            ReacquireTarget();
            return;
        }
        
        if (Time.time < lastDecisionTime + decisionInterval) return;
        
        lastDecisionTime = Time.time;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        float healthPercentage = currentHealth / maxHealth;
        
        // Adjust weights based on situation
        float currentAttackWeight = attackWeight;
        float currentEvadeWeight = evadeWeight;
        
        // Low health increases evade chance
        if (healthPercentage < 0.3f)
        {
            currentEvadeWeight *= 2f;
            currentAttackWeight *= 0.5f;
        }
        
        // Choose behavior based on distance and weights
        if (distanceToPlayer <= attackRange)
        {
            // In attack range
            currentState = AIState.Attack;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // In detection range - choose behavior
            float randomValue = Random.Range(0f, 1f);
            
            if (randomValue < currentAttackWeight)
            {
                currentState = AIState.Chase;
            }
            else if (randomValue < currentAttackWeight + currentEvadeWeight)
            {
                currentState = AIState.Evade;
            }
            else
            {
                currentState = AIState.Wander;
            }
        }
        else
        {
            // Out of range
            currentState = AIState.Wander;
        }
        
        Debug.Log($"AI Decision: State={currentState}, Distance={distanceToPlayer:F1}, Health={healthPercentage:P0}");
    }
    
    void UpdateState()
    {
        if (!hasValidTarget || playerTarget == null)
        {
            currentState = AIState.Wander;
        }
        
        switch (currentState)
        {
            case AIState.Idle:
                IdleBehavior();
                break;
                
            case AIState.Chase:
                ChaseBehavior();
                break;
                
            case AIState.Attack:
                AttackBehavior();
                break;
                
            case AIState.Evade:
                EvadeBehavior();
                break;
                
            case AIState.Wander:
                WanderBehavior();
                break;
        }
    }
    
    void IdleBehavior()
    {
        movementDirection = Vector3.zero;
        if (navMeshAgent != null)
            navMeshAgent.isStopped = true;
    }
    
    void ChaseBehavior()
    {
        if (playerTarget == null) return;
        
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        movementDirection = directionToPlayer;
        
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(playerTarget.position);
        }
    }
    
    void AttackBehavior()
    {
        movementDirection = Vector3.zero;
        
        if (Time.time > lastAttackTime + attackCooldown)
        {
            ChooseAttack();
            lastAttackTime = Time.time;
        }
        
        // Face the player
        if (playerTarget != null)
        {
            Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        if (navMeshAgent != null)
            navMeshAgent.isStopped = true;
    }
    
    void EvadeBehavior()
    {
        if (playerTarget == null) return;
        
        Vector3 directionFromPlayer = (transform.position - playerTarget.position).normalized;
        movementDirection = directionFromPlayer;
        
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            Vector3 evadePoint = transform.position + directionFromPlayer * 10f;
            navMeshAgent.SetDestination(evadePoint);
        }
    }
    
    void WanderBehavior()
    {
        if (Vector3.Distance(transform.position, wanderPoint) < 2f)
        {
            wanderPoint = GetWanderPoint();
        }
        
        Vector3 directionToWander = (wanderPoint - transform.position).normalized;
        movementDirection = directionToWander;
        
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(wanderPoint);
        }
    }
    
    Vector3 GetWanderPoint()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        randomPoint.y = 0;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return transform.position;
    }
    
    void ChooseAttack()
    {
        if (abilitySystem == null) 
        {
            Debug.LogWarning("AbilitySystem is null in AIDragon");
            return;
        }
        
        // Choose random ability that's ready
        int[] abilityIndices = new int[] { 0, 1, 2 };
        ShuffleArray(abilityIndices);
        
        foreach (int index in abilityIndices)
        {
            if (abilitySystem.IsAbilityReady(index))
            {
                UseAbility(index);
                break;
            }
        }
    }
    
    void UseAbility(int abilityIndex)
    {
        if (abilitySystem == null) return;
        
        if (abilitySystem.UseAbility(abilityIndex))
        {
            switch (abilityIndex)
            {
                case 0: // Fire Attack
                    if (animator != null)
                        animator.SetTrigger("FireAttack");
                    break;
                    
                case 1: // Tail Attack
                    if (animator != null)
                        animator.SetTrigger("TailAttack");
                    break;
                    
                case 2: // Fly Attack
                    if (animator != null)
                    {
                        animator.SetTrigger("FlyAttack");
                        isFlying = true;
                        if (flyAttack != null)
                            StartCoroutine(EndFlying(flyAttack.flyDuration));
                        else
                            StartCoroutine(EndFlying(5f));
                    }
                    break;
            }
        }
    }
    
    IEnumerator EndFlying(float duration)
    {
        yield return new WaitForSeconds(duration);
        isFlying = false;
        if (animator != null)
            animator.SetTrigger("Land");
    }
    
    void ShuffleArray(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
    
    void ReacquireTarget()
    {
        Debug.Log("AIDragon: Attempting to reacquire target...");
        
        // Try different methods to find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            hasValidTarget = true;
            Debug.Log("Target reacquired by tag.");
            return;
        }
        
        // PlayerDragon player = FindObjectOfType<PlayerDragon>();
        // if (player != null)
        // {
        //     playerTarget = player.transform;
        //     hasValidTarget = true;
        //     Debug.Log("Target reacquired by component.");
        //     return;
        // }
        
        // Last resort: check GameManager
        if (GameManager.Instance != null && GameManager.Instance.playerDragon != null)
        {
            playerTarget = GameManager.Instance.playerDragon.transform;
            hasValidTarget = true;
            Debug.Log("Target reacquired from GameManager.");
            return;
        }
        
        Debug.LogWarning("Could not reacquire target. AI disabled.");
        hasValidTarget = false;
    }
    
    void CreateDummyTarget()
    {
        // Create a dummy target for testing
        GameObject dummy = new GameObject("DummyPlayerTarget");
        dummy.transform.position = transform.position + new Vector3(10, 0, 0);
        playerTarget = dummy.transform;
        hasValidTarget = true;
        Debug.Log("Created dummy target for AI testing.");
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
        
        // Initialize ability system
        if (abilitySystem != null)
        {
            abilitySystem.Initialize(new AbilityBase[] { fireAttack, tailAttack, flyAttack });
        }
        else
        {
            Debug.LogError("AbilitySystem is null in AIDragon!");
        }
    }
    
    public override void ResetDragon()
    {
        base.ResetDragon();
        
        // Reset flying state
        isFlying = false;
        
        // Reset AI state
        currentState = AIState.Idle;
        
        // Reset abilities
        if (abilitySystem != null)
        {
            abilitySystem.ResetAllAbilities();
        }
        
        // Reset navigation
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }
        
        // Reinitialize
        InitializeAI();
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw wander point
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(wanderPoint, 0.5f);
        Gizmos.DrawLine(transform.position, wanderPoint);
        
        // Draw line to player if exists
        if (playerTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
        
        // Draw current state
        // UnityEditor.Handles.Label(transform.position + Vector3.up * 5f, 
        //                          $"State: {currentState}\nHealth: {currentHealth}/{maxHealth}");
    }
}