using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Dragon References")]
    public PlayerDragon playerDragon;
    public AIDragon aiDragon;
    
    [Header("Game Settings")]
    public float gameSpeed = 1f;
    public bool isGameOver = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        Time.timeScale = gameSpeed;
    }
    
    public void DragonDied(DragonController dragon)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        // Determine winner
        DragonController winner = (dragon == playerDragon) ? aiDragon : playerDragon;
        
        // Show winner screen
        UIManager.Instance.ShowWinnerScreen(winner.dragonName, winner == playerDragon);
        
        // Slow motion effect
        StartCoroutine(SlowMotionEffect());
    }
    
    private IEnumerator SlowMotionEffect()
    {
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            Time.timeScale = Mathf.Lerp(1f, 0.3f, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        Time.timeScale = 0.3f;
    }
    
    public void RestartGame()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        
        // Reset dragons
        playerDragon.ResetDragon();
        aiDragon.ResetDragon();
        
        // Hide winner screen
        UIManager.Instance.HideWinnerScreen();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}