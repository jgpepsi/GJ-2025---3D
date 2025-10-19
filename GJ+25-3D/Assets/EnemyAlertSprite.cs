using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class EnemyAlertSprite : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Sprite exibido quando não há inimigos dentro da área.")]
    public Sprite defaultSprite;

    [Tooltip("Sprite exibido quando há pelo menos 1 inimigo dentro da área.")]
    public Sprite enemySprite;

    [Header("Referências")]
    [Tooltip("Imagem que receberá os sprites.")]
    public Image targetImage;

    // Lista para armazenar os inimigos que estão dentro da área
    private List<GameObject> enemiesInside = new List<GameObject>();

    private void Start()
    {
        // Garante que comece com o sprite padrão
        if (targetImage != null && defaultSprite != null)
        {
            targetImage.sprite = defaultSprite;
        }

        // Garante que o BoxCollider esteja configurado como Trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        // Remove referências nulas (caso inimigos sejam destruídos dentro da área)
        enemiesInside.RemoveAll(enemy => enemy == null);

        // Atualiza sprite de acordo com a quantidade de inimigos ativos
        if (enemiesInside.Count > 0)
        {
            if (targetImage.sprite != enemySprite && enemySprite != null)
                targetImage.sprite = enemySprite;
        }
        else
        {
            if (targetImage.sprite != defaultSprite && defaultSprite != null)
                targetImage.sprite = defaultSprite;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem entrou tem a tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInside.Contains(other.gameObject))
            {
                enemiesInside.Add(other.gameObject);
            }

            // Troca o sprite assim que detecta o inimigo
            if (targetImage != null && enemySprite != null)
            {
                targetImage.sprite = enemySprite;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verifica se quem saiu é um inimigo
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInside.Contains(other.gameObject))
            {
                enemiesInside.Remove(other.gameObject);
            }

            // Se não houver mais inimigos, volta ao sprite padrão
            if (enemiesInside.Count == 0 && targetImage != null && defaultSprite != null)
            {
                targetImage.sprite = defaultSprite;
            }
        }
    }
}
