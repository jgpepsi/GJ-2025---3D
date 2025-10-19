using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Painel de Game Over que será ativado ao morrer.")]
    public GameObject gameOverPanel;

    private bool isGameOver = false;

    void Start()
    {
        // Garante que o painel começa desativado
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Trava o cursor no começo do jogo (modo gameplay)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Se o jogo acabou, permite reiniciar com R
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    /// <summary>
    /// Ativa o painel de Game Over e libera o mouse.
    /// Chame esta função quando o jogador morrer.
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // pausa o jogo
        isGameOver = true;

        // Libera o cursor para clicar nos botões
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Reinicia a cena atual.
    /// </summary>
    public void RestartScene()
    {
        Time.timeScale = 1f; // retoma o tempo antes de recarregar
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
