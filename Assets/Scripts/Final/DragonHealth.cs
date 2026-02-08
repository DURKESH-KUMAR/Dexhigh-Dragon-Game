using UnityEngine;
using UnityEngine.UI;

public class DragonHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Health Bar (Slider)")]
    public Slider healthSlider;   // Assign in Inspector

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log(gameObject.name + " took " + damage + " damage");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log(gameObject.name + " defeated");
        gameObject.SetActive(false);
    }
}
