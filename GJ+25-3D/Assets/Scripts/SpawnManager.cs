using NUnit.Framework;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

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
    public float spawnInterval;
    public float minSpawnInterval;
    public List<EnemyChance> chancesList = new List<EnemyChance>();
    
    private int wave = 0;
    private int waveProgress;
    private int waveGoal;

    private int chanceSum;

    void Start()
    {
        StartCoroutine(SpawnNext());
    }

    public void SpawnEnemy()
    {
        if (Random.Range(0,2) > 0)
        {
            Instantiate(GetRandomEnemy(), spawnPosRight.position, Quaternion.identity);
        }
        else
        {
            Instantiate(GetRandomEnemy(), spawnPosLeft.position, Quaternion.identity);
        }
    }

    public IEnumerator SpawnNext()
    {
        SpawnEnemy();
        yield return new WaitForSeconds(spawnInterval);
        StartCoroutine(SpawnNext());
    }

    public GameObject GetRandomEnemy()
    {
        int randNum = Random.Range(0, chanceSum-1);
        int cumulativeChance = 0;
        foreach (EnemyChance chance in chancesList)
        {
            cumulativeChance += chance.chance;
            if(cumulativeChance > randNum) {
                return chance.enemy;
            }
        }
        return chancesList[0].enemy;
    }

    public void NextWave()
    {
        wave++;
        if (spawnInterval >= minSpawnInterval)
        {
            spawnInterval--;
        }
        
        chanceSum = 0;

        foreach (EnemyChance chance in chancesList)
        {
            chanceSum += chance.chance;
        }

        chancesList[Random.Range(1, 3)].chance++;
    }
}
