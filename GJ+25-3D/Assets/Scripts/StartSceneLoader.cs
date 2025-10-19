using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StartSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;

    [SerializeField] private Image flashImage;
    [SerializeField] private Image fadePanel;

    private bool hasStarted = false;

    void Update()
    {
        if (!hasStarted && Input.anyKeyDown)
        {
            hasStarted = true;
            StartCoroutine(SequenciaDeInicio());
        }
    }

    private IEnumerator SequenciaDeInicio()
    {
        yield return StartCoroutine(FlicarImagem());
        yield return StartCoroutine(FadePainel());

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FlicarImagem()
    {
        float intervalo = 0.2f;
        Color normalColor = Color.white;
        Color preto = Color.black;

        flashImage.color = preto;
        yield return new WaitForSeconds(intervalo);

        flashImage.color = normalColor;
        yield return new WaitForSeconds(intervalo);

        flashImage.color = preto;
        yield return new WaitForSeconds(intervalo);

        flashImage.color = normalColor;
        yield return new WaitForSeconds(intervalo);
    }

    private IEnumerator FadePainel()
    {
        float duration = 1.5f;
        float t = 0f;
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 1);

        while (t < duration)
        {
            t += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, endColor, t / duration);
            yield return null;
        }

        fadePanel.color = endColor;
    }
}