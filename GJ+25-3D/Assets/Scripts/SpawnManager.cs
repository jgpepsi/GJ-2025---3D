using NUnit.Framework;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyChance
{
    public GameObject enemy;
    public int chance;
}

public class SpawnManager : MonoBehaviour
{
    public Transform spawnPosRight, spawnPosLeft;
    public GameObject enemyPrefab;
    public float spawnInterval, spawnInterval2;
    public List<EnemyChance> chancesList = new List<EnemyChance>();
    private int wave = 0;

    void Start()
    {
        StartCoroutine(SpawnNext());
    }

    public void SpawnEnemy(bool spawnRight)
    {
        if (spawnRight)
        {
            Instantiate(enemyPrefab, spawnPosRight.position, Quaternion.identity);
        }
        else
        {
            Instantiate(enemyPrefab, spawnPosLeft.position, Quaternion.identity);
        }
    }

    public IEnumerator SpawnNext()
    {
        SpawnEnemy(true);
        yield return new WaitForSeconds(spawnInterval);
        SpawnEnemy(false);
        yield return new WaitForSeconds(spawnInterval2);
        StartCoroutine(SpawnNext());
    }
}
