using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [Header("Ability Settings")]
    public string abilityName = "Ability";
    public float cooldown = 3f;
    public float manaCost = 10f;
    public float range = 5f;
    public KeyCode hotkey = KeyCode.None;
    
    [Header("Visual Effects")]
    public GameObject castEffect;
    public AudioClip castSound;
    
    [HideInInspector]
    public DragonController owner;
    
    protected bool isActive = false;
    protected float activeTime = 0f;
    
    public virtual void Initialize(DragonController dragon)
    {
        owner = dragon;
    }
    
    public virtual bool CanUse()
    {
        if (owner == null) return false;
        if (owner.isDead) return false;
        if (isActive) return false;
        
        return true;
    }
    
    public virtual bool Use()
    {
        if (!CanUse()) return false;
        
        isActive = true;
        activeTime = 0f;
        
        // Play effects
        PlayEffects();
        
        return true;
    }
    
    protected virtual void Update()
    {
        if (isActive)
        {
            activeTime += Time.deltaTime;
            OnAbilityActive();
        }
    }
    
    protected virtual void OnAbilityActive()
    {
        // Override in child classes
    }
    
    protected virtual void EndAbility()
    {
        isActive = false;
        activeTime = 0f;
    }
    
    protected virtual void PlayEffects()
    {
        if (castEffect && owner.attackPoint)
        {
            GameObject effect = Instantiate(castEffect, owner.attackPoint.position, owner.attackPoint.rotation);
            Destroy(effect, 3f);
        }
        
        if (castSound && owner.audioSource)
        {
            owner.audioSource.PlayOneShot(castSound);
        }
    }
    
    protected DragonController FindNearestEnemy()
    {
        DragonController[] dragons = FindObjectsOfType<DragonController>();
        DragonController nearest = null;
        float nearestDistance = Mathf.Infinity;
        
        foreach (DragonController dragon in dragons)
        {
            if (dragon == owner || dragon.isDead) continue;
            
            float distance = Vector3.Distance(owner.transform.position, dragon.transform.position);
            if (distance < nearestDistance && distance <= range)
            {
                nearestDistance = distance;
                nearest = dragon;
            }
        }
        
        return nearest;
    }
}