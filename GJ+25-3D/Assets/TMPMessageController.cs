using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TMPMessageController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao TMP_Text que exibirá as mensagens.")]
    public TMP_Text messageText;

    [Header("Configuração de Mensagens")]
    [Tooltip("Lista de mensagens que podem ser exibidas.")]
    public List<string> messages = new List<string>();

    [Tooltip("Tempo em segundos que a mensagem ficará ativa.")]
    public float displayTime = 2f;

    [Header("Configuração do Bounce")]
    [Tooltip("Tamanho máximo da escala no efeito bounce.")]
    public float bounceScale = 1.3f;
    [Tooltip("Velocidade de oscilação do bounce.")]
    public float bounceSpeed = 3f;

    private Coroutine hideCoroutine;
    private Coroutine bounceCoroutine;
    public Vector3 originalScale;

    void Start()
    {
        // Garante que o texto começa invisível
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
            originalScale = messageText.rectTransform.localScale;
        }
    }

    void Update()
    {
        // Verifica se a tecla 1 foi pressionada
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowMessageByIndex(0); // primeira mensagem
        }

        // Verifica se a tecla 2 foi pressionada
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowMessageByIndex(1); // segunda mensagem
        }

        // Você pode adicionar mais teclas facilmente:
        // Exemplo:
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        //     ShowMessageByIndex(2);
    }

    /// <summary>
    /// Mostra uma mensagem pelo índice na lista e inicia o timer de esconder.
    /// </summary>
    /// <param name="index">Índice da mensagem na lista.</param>
    public void ShowMessageByIndex(int index)
    {
        if (messageText == null || index < 0 || index >= messages.Count)
            return;

        messageText.text = messages[index];
        messageText.gameObject.SetActive(true);

        // Se já havia uma coroutine rodando, para ela
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (bounceCoroutine != null)
            StopCoroutine(bounceCoroutine);

        // Inicia as coroutines de bounce e hide
        bounceCoroutine = StartCoroutine(BounceEffect());
        hideCoroutine = StartCoroutine(HideMessageAfterTime());
    }

    /// <summary>
    /// Esconde o texto após 'displayTime' segundos.
    /// </summary>
    private IEnumerator HideMessageAfterTime()
    {
        yield return new WaitForSeconds(displayTime);
        messageText.gameObject.SetActive(false);

        // Reseta a escala
        messageText.rectTransform.localScale = originalScale;

        hideCoroutine = null;

        // Para o bounce
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }
    }

    /// <summary>
    /// Faz o texto "pulsar" com efeito bounce enquanto estiver ativo.
    /// </summary>
    private IEnumerator BounceEffect()
    {
        float timer = 1f;
        while (messageText.gameObject.activeSelf)
        {
            float scale = Mathf.Lerp(1f, bounceScale, (Mathf.Sin(timer * bounceSpeed) + 1f) / 2f);
            messageText.rectTransform.localScale = originalScale * scale;

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
