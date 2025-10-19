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
    public GameObject feed;

    [Tooltip("Aumenta o tamanho da fonte ao alcançar múltiplos de 5.")]
    public int multipleOf5 = 5;

    [Tooltip("Aumenta ainda mais ao alcançar múltiplos de 10.")]
    public int multipleOf10 = 10;

    [Header("Animação de Tamanho")]
    [Tooltip("Tamanho original da fonte.")]
    public float originalFontSize = 36f;

    [Tooltip("Tamanho da fonte ao atingir múltiplos de 5.")]
    public float fontSizeOn5 = 42f;

    [Tooltip("Tamanho da fonte ao atingir múltiplos de 10.")]
    public float fontSizeOn10 = 50f;

    [Tooltip("Duração da animação de aumento em segundos.")]
    public float sizeAnimationDuration = 0.5f;

    private Coroutine sizeCoroutine;

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
        NewUpdateScoreUI();
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScoreUI());
    }

    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    private void NewUpdateScoreUI()
    {
        scoreText.text = score.ToString();

        if (score % multipleOf10 == 0)
        {
            FindObjectOfType<TMPMessageController>().ShowMessageByIndex(0);
        }
        else if (score % multipleOf5 == 0)
        {
            FindObjectOfType<TMPMessageController>().ShowMessageByIndex(0);
        }
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