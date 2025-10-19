using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives;

    [SerializeField] private Image[] lifeImages;
    [SerializeField] private Color fullColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Pontuação")]
    [SerializeField] private int pointsPerEnemy = 10;
    private int score = 0;

    [SerializeField] private TextMeshProUGUI scoreText;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        originalScale = scoreText.transform.localScale;
        currentLives = maxLives;
        UpdateLifeImages();
        UpdateScoreUI();
    }
    public void TakeDamage()
    {
        if (currentLives <= 0) return;

        currentLives--;
        UpdateLifeImages();

        if (currentLives <= 0)
        {
            Debug.Log("Game Over!");
        }
    }    

    private void UpdateLifeImages()
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (i < currentLives)
                lifeImages[i].color = fullColor;
            else
                lifeImages[i].color = emptyColor;
        }
    }

    public void AddScore()
    {
        score += pointsPerEnemy;
        UpdateScoreUI();
                
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScoreUI());
    }

    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

   
    private IEnumerator AnimateScoreUI()
    {
        Vector3 targetScale = originalScale * 1.5f;
        float duration = 0.1f;
        float t = 0f;

        // aumenta de tamanho
        while (t < duration)
        {
            scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        scoreText.transform.localScale = targetScale;

        // volta ao tamanho
        t = 0f;
        while (t < duration)
        {
            scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        scoreText.transform.localScale = originalScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage();

        if (Input.GetKeyDown(KeyCode.K))
            AddScore();
    }
}