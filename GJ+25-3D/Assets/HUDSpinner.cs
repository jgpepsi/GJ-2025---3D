using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HUDSpinner : MonoBehaviour
{
    [Header("Posicionamento")]
    public Vector2 startAnchoredPosition = new Vector2(0f, -200f);
    public Vector2 targetAnchoredPosition = new Vector2(0f, 200f);

    [Header("Movimento")]
    public float moveSpeed = 50f;

    [Header("Oscilação de Scale X")]
    public float oscillationFrequency = 2f;
    public float oscillationAmplitude = 1f;
    public bool positionAtStart = true;

    [Header("Animação de Escala")]
    [Tooltip("Fator máximo de crescimento da escala.")]
    public float maxScale = 1.5f;
    [Tooltip("Fator mínimo ao reduzir a escala.")]
    public float minScale = 0.2f;
    [Tooltip("Tempo de animação de crescimento/encolhimento.")]
    public float scaleDuration = 1f;

    [SerializeField] private RectTransform rt;
    [SerializeField] private Image image;

    private bool isMoving = false;
    private Vector3 originalLocalScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        originalLocalScale = rt.localScale;
    }

    void Start()
    {
        if (positionAtStart)
            rt.anchoredPosition = startAnchoredPosition;

        if (Vector2.Distance(rt.anchoredPosition, targetAnchoredPosition) <= 0.01f)
        {
            isMoving = false;
            rt.localScale = new Vector3(1f, originalLocalScale.y, originalLocalScale.z);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            isMoving = true;

        if (!isMoving) return;

        // Movimento até o destino
        Vector2 current = rt.anchoredPosition;
        Vector2 next = Vector2.MoveTowards(current, targetAnchoredPosition, moveSpeed * Time.deltaTime);
        rt.anchoredPosition = next;

        // Oscilação do eixo X
        float sin = Mathf.Sin(Time.time * oscillationFrequency * Mathf.PI * 2f) * Mathf.Clamp01(oscillationAmplitude);
        float scaleX = Mathf.Clamp(sin, -1f, 1f);

        Vector3 s = rt.localScale;
        s.x = scaleX;
        s.y = originalLocalScale.y;
        s.z = originalLocalScale.z;
        rt.localScale = s;

        // Verifica chegada
        if (Vector2.Distance(next, targetAnchoredPosition) <= 0.01f)
        {
            isMoving = false;
            rt.localScale = originalLocalScale;
            rt.anchoredPosition = targetAnchoredPosition;
        }
    }

    public void MoveSpinner()
    {
        StartCoroutine(MoveSpinnerRoutine());
        isMoving = false;
    }

    private IEnumerator MoveSpinnerRoutine()
    {
        // --- 1. Aumenta gradualmente a escala ---
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            float scaleValue = Mathf.Lerp(1f, maxScale, t);
            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(maxScale, maxScale, maxScale);

        // --- 2. Diminui gradualmente até minScale ---
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            float scaleValue = Mathf.Lerp(maxScale, minScale, t);
            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(minScale, minScale, minScale);

        rt.anchoredPosition = new Vector2(-100f, -500f);

        // --- 3. Volta à escala original ---
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            float scaleValue = Mathf.Lerp(minScale, 1f, t);
            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalLocalScale;

    }
}
