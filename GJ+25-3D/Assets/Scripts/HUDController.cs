using JetBrains.Annotations;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives;
    [SerializeField] private HUDSpinner spinner;

    [SerializeField] private Image[] lifeImages;
    [SerializeField] private Color fullColor = Color.white;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Pontuacao")]
    [SerializeField] private int pointsPerEnemy = 10;
    private int score = 0;

    [SerializeField] private TextMeshProUGUI scoreText;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    [Header("Configuração do Shake")]
    [Tooltip("Duracao total do tremor (em segundos)")]
    public float shakeDuration = 0.2f;

    [Tooltip("Intensidade do tremor")]
    public float shakeAmount = 5f;

    public RectTransform imageToShake;
    private Vector3 originalPosition;

    void Start()
    {
        originalScale = scoreText.transform.localScale;
        currentLives = maxLives;
        UpdateLifeImages();
        UpdateScoreUI();

        if (imageToShake != null)
        {
            originalPosition = imageToShake.anchoredPosition;
        }
        else
        {
            Debug.LogWarning(" Nenhuma imagem foi atribuída para tremer!");
        }
    }
    public void TakeDamage()
    {
        if (currentLives <= 0) return;

        currentLives--;
        UpdateLifeImages();

        if (currentLives <= 0)
        {
            Debug.Log("Game Over!");
            imageToShake.gameObject.SetActive(false);
        }
        StartShake();
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
        if (Input.GetKeyDown(KeyCode.D))
            spinner.MoveSpinner();

        UpdateLifeImages();
    }

    public void StartShake()
    {
        if (imageToShake != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeAmount;
            float offsetY = Random.Range(-1f, 1f) * shakeAmount;

            imageToShake.anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        imageToShake.anchoredPosition = originalPosition;
    }

}