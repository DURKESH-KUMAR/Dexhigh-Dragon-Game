using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityIcon : MonoBehaviour
{
    [Header("References")]
    public Image abilityIcon;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI hotkeyText;
    
    [Header("Settings")]
    public int abilityIndex = 0;
    public KeyCode hotkey = KeyCode.Q;
    
    private AbilitySystem abilitySystem;
    
    void Start()
    {
        // Set hotkey text
        if (hotkeyText)
            hotkeyText.text = hotkey.ToString();
        
        // Find ability system (assuming player dragon)
        PlayerDragon playerDragon = FindObjectOfType<PlayerDragon>();
        if (playerDragon)
        {
            abilitySystem = playerDragon.GetComponent<AbilitySystem>();
        }
    }
    
    void Update()
    {
        if (abilitySystem == null) return;
        
        float cooldownProgress = abilitySystem.GetCooldownProgress(abilityIndex);
        bool isReady = abilitySystem.IsAbilityReady(abilityIndex);
        
        UpdateCooldown(cooldownProgress, isReady);
    }
    
    public void UpdateCooldown(float progress, bool isReady)
    {
        if (cooldownOverlay)
        {
            cooldownOverlay.fillAmount = progress;
            cooldownOverlay.gameObject.SetActive(!isReady);
        }
        
        if (cooldownText)
        {
            if (!isReady && abilitySystem != null)
            {
                float remainingCooldown = abilitySystem.GetCooldownProgress(abilityIndex) * 
                    abilitySystem.abilities[abilityIndex].cooldown;
                cooldownText.text = Mathf.CeilToInt(remainingCooldown).ToString();
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
        
        // Visual feedback for ready ability
        if (abilityIcon)
        {
            abilityIcon.color = isReady ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}