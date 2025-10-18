using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform spawnPosRight, spawnPosLeft;
    public GameObject enemyPrefab;
    public float spawnInterval, spawnInterval2;

    void Start()
    {
        StartCoroutine(SpawnNext());
    }

   
    void Update()
    {
        
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
