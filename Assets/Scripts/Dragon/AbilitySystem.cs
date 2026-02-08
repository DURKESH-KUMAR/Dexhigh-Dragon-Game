using UnityEngine;
using System.Collections;

public class AbilitySystem : MonoBehaviour
{
    public AbilityBase[] abilities;
    
    private float[] cooldownTimers;
    private bool[] isOnCooldown;
    
    public void Initialize(AbilityBase[] abilityArray)
    {
        abilities = abilityArray;
        cooldownTimers = new float[abilities.Length];
        isOnCooldown = new bool[abilities.Length];
        
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null)
            {
                abilities[i].owner = GetComponent<DragonController>();
            }
        }
    }
    
    void Update()
    {
        // Update cooldowns
        for (int i = 0; i < abilities.Length; i++)
        {
            if (isOnCooldown[i])
            {
                cooldownTimers[i] -= Time.deltaTime;
                if (cooldownTimers[i] <= 0)
                {
                    isOnCooldown[i] = false;
                }
            }
        }
    }
    
    public bool UseAbility(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Length) return false;
        if (abilities[abilityIndex] == null) return false;
        if (isOnCooldown[abilityIndex]) return false;
        
        // Check if ability can be used
        if (abilities[abilityIndex].CanUse())
        {
            bool used = abilities[abilityIndex].Use();
            
            if (used)
            {
                // Start cooldown
                StartCooldown(abilityIndex, abilities[abilityIndex].cooldown);
                return true;
            }
        }
        
        return false;
    }
    
    public void StartCooldown(int abilityIndex, float cooldownDuration)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Length) return;
        
        cooldownTimers[abilityIndex] = cooldownDuration;
        isOnCooldown[abilityIndex] = true;
    }
    
    public float GetCooldownProgress(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Length) return 0f;
        if (!isOnCooldown[abilityIndex]) return 0f;
        
        return cooldownTimers[abilityIndex] / abilities[abilityIndex].cooldown;
    }
    
    public bool IsAbilityReady(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Length) return false;
        return !isOnCooldown[abilityIndex];
    }
    
    public void ResetAllAbilities()
    {
        for (int i = 0; i < isOnCooldown.Length; i++)
        {
            isOnCooldown[i] = false;
            cooldownTimers[i] = 0f;
        }
    }
}