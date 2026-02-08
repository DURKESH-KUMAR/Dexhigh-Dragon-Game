using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinnerScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI winnerText;
    public Image winnerImage;
    public Button restartButton;
    public Button quitButton;
    public GameObject victoryParticles;
    
    [Header("Sprites")]
    public Sprite playerWinSprite;
    public Sprite aiWinSprite;
    
    void Start()
    {
        // Hide on start
        gameObject.SetActive(false);
        
        // Setup button listeners
        if (restartButton)
            restartButton.onClick.AddListener(OnRestart);
        
        if (quitButton)
            quitButton.onClick.AddListener(OnQuit);
    }
    
    public void ShowWinner(string winnerName, bool playerWon)
    {
        gameObject.SetActive(true);
        
        // Update text
        if (winnerText)
            winnerText.text = $"{winnerName} Wins!";
        
        // Update image
        if (winnerImage)
            winnerImage.sprite = playerWon ? playerWinSprite : aiWinSprite;
        
        // Show particles
        if (victoryParticles)
            victoryParticles.SetActive(true);
        
        // Play sound
        if (AudioManager.Instance != null)
{
    AudioManager.Instance.PlayVictorySound();
}
        
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        
        if (victoryParticles)
            victoryParticles.SetActive(false);
    }
    
    void OnRestart()
    {
        GameManager.Instance.RestartGame();
    }
    
    void OnQuit()
    {
        GameManager.Instance.QuitGame();
    }
}