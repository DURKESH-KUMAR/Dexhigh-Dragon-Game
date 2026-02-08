using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Health Bars")]
    public Slider playerHealthBar;
    public Slider aiHealthBar;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI aiHealthText;
    public Image playerHealthFill;
    public Image aiHealthFill;
    
    [Header("Ability UI")]
    public AbilityIcon[] abilityIcons;
    public TextMeshProUGUI cooldownText;
    
    [Header("Winner Screen")]
    public GameObject winnerScreen;
    public TextMeshProUGUI winnerText;
    public Image winnerDragonImage;
    public Sprite playerWinSprite;
    public Sprite aiWinSprite;
    
    [Header("Colors")]
    public Color highHealthColor = Color.green;
    public Color mediumHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void UpdateHealthBar(bool isPlayer, float currentHealth, float maxHealth)
    {
        Slider healthBar = isPlayer ? playerHealthBar : aiHealthBar;
        Image healthFill = isPlayer ? playerHealthFill : aiHealthFill;
        TextMeshProUGUI healthText = isPlayer ? playerHealthText : aiHealthText;
        
        float healthPercentage = currentHealth / maxHealth;
        healthBar.value = healthPercentage;
        healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        
        // Update color based on health percentage
        if (healthPercentage > 0.6f)
            healthFill.color = highHealthColor;
        else if (healthPercentage > 0.3f)
            healthFill.color = mediumHealthColor;
        else
            healthFill.color = lowHealthColor;
    }
    
    public void UpdateAbilityUI(int abilityIndex, float cooldownProgress, bool isReady)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityIcons.Length) return;
        
        abilityIcons[abilityIndex].UpdateCooldown(cooldownProgress, isReady);
    }
    
    public void ShowWinnerScreen(string winnerName, bool playerWon)
    {
        winnerText.text = $"{winnerName} Wins!";
        winnerDragonImage.sprite = playerWon ? playerWinSprite : aiWinSprite;
        winnerScreen.SetActive(true);
    }
    
    public void HideWinnerScreen()
    {
        winnerScreen.SetActive(false);
    }
    
    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }
    
    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }
}