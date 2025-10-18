using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Life")]
    [SerializeField] private float maxHealth = 100f; 
    private float currentHealth;

    [Header("Score")]
    [SerializeField] private int ScoreForEnemy = 10;
    private int score = 0;

    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI scoreText;

    void Start()
    {
        // Inicializa vida e UI
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        UpdateScoreUI();
    }
        
    /// Atualiza a vida do jogador
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.value = currentHealth;
    }

    /// Função chamada quando um inimigo morre
    public void AddScore()
    {
        score += ScoreForEnemy;
        UpdateScoreUI();
    }

    /// Atualiza o texto do score na tela
    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score.ToString();
    }

}
