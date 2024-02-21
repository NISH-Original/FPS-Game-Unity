using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    GameObject enemy;
    public Transform[] spawnPoints;

    public float respawnTime;

    public void TriggerRespawn(GameObject enemy)
    {
        StartCoroutine(SpawnEnemy(enemy));
    }

    public IEnumerator SpawnEnemy(GameObject enemy)
    {
        yield return new WaitForSeconds(respawnTime);

        // get a random spawnpoint from list
        Transform respawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; 
        enemy.SetActive(true);
        enemy.GetComponent<Transform>().position = respawnPoint.position;
    }
}
