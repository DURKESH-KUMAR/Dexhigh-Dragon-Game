using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;
    
    [Header("Settings")]
    public bool isPlayer = true;
    public float updateSpeed = 5f;
    
    private DragonController targetDragon;
    private float targetValue;
    
    void Start()
    {
        // Find the target dragon
        if (isPlayer)
        {
            targetDragon = FindObjectOfType<PlayerDragon>();
        }
        else
        {
            targetDragon = FindObjectOfType<AIDragon>();
        }
        
        if (targetDragon)
        {
            if (nameText)
                nameText.text = targetDragon.dragonName;
            
            // Subscribe to health changes
            targetDragon.OnHealthChanged += OnHealthChanged;
            
            // Initial update
            OnHealthChanged(targetDragon.currentHealth / targetDragon.maxHealth);
        }
    }
    
    void OnDestroy()
    {
        if (targetDragon != null)
        {
            targetDragon.OnHealthChanged -= OnHealthChanged;
        }
    }
    
    void OnHealthChanged(float healthPercentage)
    {
        targetValue = healthPercentage;
    }
    
    void Update()
    {
        // Smoothly update health bar
        if (healthSlider)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, updateSpeed * Time.deltaTime);
            
            // Update text
            if (healthText && targetDragon)
            {
                float currentHealth = targetValue * targetDragon.maxHealth;
                healthText.text = $"{Mathf.RoundToInt(currentHealth)}/{targetDragon.maxHealth}";
            }
            
            // Update color
            if (fillImage)
            {
                if (targetValue > 0.6f)
                    fillImage.color = Color.green;
                else if (targetValue > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }
    }
}