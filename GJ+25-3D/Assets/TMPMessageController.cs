using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TMPMessageController : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Refer�ncia ao TMP_Text que exibir� as mensagens.")]
    public TMP_Text messageText;

    [Header("Configura��o de Mensagens")]
    [Tooltip("Lista de mensagens que podem ser exibidas.")]
    public List<string> messages = new List<string>();

    [Tooltip("Tempo em segundos que a mensagem ficar� ativa.")]
    public float displayTime = 2f;

    private Coroutine hideCoroutine;

    void Start()
    {
        // Garante que o texto come�a invis�vel
        if (messageText != null)
            messageText.gameObject.SetActive(false);
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

        // Voc� pode adicionar mais teclas facilmente:
        // Exemplo:
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        //     ShowMessageByIndex(2);
    }

    /// <summary>
    /// Mostra uma mensagem pelo �ndice na lista e inicia o timer de esconder.
    /// </summary>
    /// <param name="index">�ndice da mensagem na lista.</param>
    public void ShowMessageByIndex(int index)
    {
        if (messageText == null || index < 0 || index >= messages.Count)
            return;

        messageText.text = messages[index];
        messageText.gameObject.SetActive(true);

        // Se j� havia uma coroutine rodando, para ela
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        // Inicia a contagem para esconder o texto
        hideCoroutine = StartCoroutine(HideMessageAfterTime());
    }

    /// <summary>
    /// Esconde o texto ap�s 'displayTime' segundos.
    /// </summary>
    private IEnumerator HideMessageAfterTime()
    {
        yield return new WaitForSeconds(displayTime);
        messageText.gameObject.SetActive(false);
        hideCoroutine = null;
    }
}
