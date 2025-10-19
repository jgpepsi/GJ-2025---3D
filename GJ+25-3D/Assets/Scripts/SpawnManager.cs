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
    private CardSpawnController cardSpawnController;

    [SerializeField]
    private int wave = 0;
    [SerializeField]
    private int waveProgress;
    [SerializeField]
    private int waveGoal = 10;

    [SerializeField]
    private int chanceSum;

    void Start()
    {
        NextWave();
        StartCoroutine(SpawnNext());
        cardSpawnController = CardSpawnController.Instance;
    }

    public void SpawnEnemy()
    {
        float checkRadius = 0.25f;
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");

        bool rightBlocked = Physics.CheckSphere(spawnPosRight.position, checkRadius, enemyLayerMask);
        bool leftBlocked = Physics.CheckSphere(spawnPosLeft.position, checkRadius, enemyLayerMask);

        if (Random.Range(0, 2) > 0)
        {
            if (!rightBlocked)
            {
                var enemy = Instantiate(GetRandomEnemy(), spawnPosRight.position, Quaternion.identity);
                enemy.GetComponent<EnemyScript>().spawnManager = this;
            }
        }
        else
        {
            if (!leftBlocked)
            {
                var enemy = Instantiate(GetRandomEnemy(), spawnPosLeft.position, Quaternion.identity);
                enemy.GetComponent<EnemyScript>().spawnManager = this;
            }
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
        waveGoal += 3;
        waveProgress = 0;
        if (spawnInterval >= minSpawnInterval)
        {
            spawnInterval -= .05f;
            if (spawnInterval <= 0.7f)
            {
                spawnInterval = 0.7f;
            }
        }
        
        
        chanceSum = 0;

        foreach (EnemyChance chance in chancesList)
        {
            chanceSum += chance.chance;
        }

        chancesList[Random.Range(1, 4)].chance++;
    }

    public void AddWaveProgress(int amount)
    {
        waveProgress += amount;
        if(waveProgress >= waveGoal)
        {
            NextWave();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPosRight.position, 0.5f);
        Gizmos.DrawSphere(spawnPosLeft.position, 0.5f);
    }
}
