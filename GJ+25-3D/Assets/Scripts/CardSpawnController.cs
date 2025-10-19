using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ListaDeCartas
{
    public List<GameObject> cardList = new List<GameObject>();
}

public class CardSpawnController : MonoBehaviour
{
    public List<GameObject> listaCartas;
    public List<ListaDeCartas> presetsCartas;
    public static CardSpawnController Instance { get; private set; }
  
    public GameObject cardContainer;
    private PlayerScript playerScript;


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        playerScript = PlayerScript.Instance;
    }

    public void ShowCardSpawner()
    {
        //Ativar objeto da UI
        cardContainer.SetActive(true);

        for (int i = 0; i < listaCartas.Count; i++)
        {
            Instantiate(listaCartas[i], cardContainer.transform);
        }
        Time.timeScale = .15f;
    }

    public void CloseCardSpawner()
    {
        cardContainer.SetActive(false);

        for (int i = 0; i < cardContainer.transform.childCount; i++)
        {
            Destroy(cardContainer.transform.GetChild(i).gameObject);
        }

        playerScript.GetComponentInChildren<SpawnManager>().NextWave();

        Time.timeScale = 1f;
    }

    public void Insert(int type)
    {
        var player = PlayerScript.Instance;

        player.InsertAttackType(type);

        CloseCardSpawner();
    }
}
