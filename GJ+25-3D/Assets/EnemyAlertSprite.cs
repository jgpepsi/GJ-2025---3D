using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class EnemyAlertSprite : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Sprite exibido quando n�o h� inimigos dentro da �rea.")]
    public Sprite defaultSprite;

    [Tooltip("Sprite exibido quando h� pelo menos 1 inimigo dentro da �rea.")]
    public Sprite enemySprite;

    [Header("Refer�ncias")]
    [Tooltip("Imagem que receber� os sprites.")]
    public Image targetImage;

    // Lista para armazenar os inimigos que est�o dentro da �rea
    private List<GameObject> enemiesInside = new List<GameObject>();

    private void Start()
    {
        // Garante que comece com o sprite padr�o
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
        // Remove refer�ncias nulas (caso inimigos sejam destru�dos dentro da �rea)
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
        // Verifica se quem saiu � um inimigo
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInside.Contains(other.gameObject))
            {
                enemiesInside.Remove(other.gameObject);
            }

            // Se n�o houver mais inimigos, volta ao sprite padr�o
            if (enemiesInside.Count == 0 && targetImage != null && defaultSprite != null)
            {
                targetImage.sprite = defaultSprite;
            }
        }
    }
}
