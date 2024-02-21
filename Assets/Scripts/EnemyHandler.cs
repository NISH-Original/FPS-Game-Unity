using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHandler : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] GameObject enemy;
    SpawnEnemies spawner;
    PlayerController pc;
    [SerializeField] Transform playerTransform;
    public float currentHealth;
    NavMeshAgent navMeshAgent;

    void OnEnable()
    {
        currentHealth = maxHealth;
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<SpawnEnemies>();
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        navMeshAgent.destination = playerTransform.position;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if(currentHealth <= 0)
        {
            spawner.TriggerRespawn(enemy);
            //pc.ShowKillMarker();
            //pc.killMarkerAudioSource.PlayOneShot(pc.killSound);
            enemy.SetActive(false);
        }
    }
}
