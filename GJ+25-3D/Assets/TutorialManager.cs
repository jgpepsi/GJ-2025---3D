using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public float timeToStartFade;
    public float fadeDuration;
    public Image button1;
    public Image button2;
    Color newColor;

    private void Start()
    {
        StartCoroutine(FadeCountdown());
    }

    private IEnumerator FadeCountdown()
    {
        yield return new WaitForSeconds(timeToStartFade);
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        newColor = button1.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            newColor.a = Mathf.Lerp(1, 0, t);
            button1.color = newColor;
            button2.color = newColor;
            yield return null;
        }
    }
}
