using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int wave;
    public float waveMod;
    public bool isWave;
    public bool allEnemiesDead;

    private int enemiesSpawned;
    public int livingEnemies;

    public int maxEnemies;
    public int minEnemies;

    public float rate;

    public GameObject enemy;
    public List<Transform> spawnPoints;

    void Start()
    {
        wave = 0;
        isWave = false;
        allEnemiesDead = true;
    }


    public void spawn()
    {
        if (isWave || !allEnemiesDead)
        {
            Debug.LogError("Wave not finished yet");
            return;
        }

        wave++;
        float playerScale = Mathf.Pow(WorldManager.singleton.players.Count,1.15f);
        int numEnemies = (int) Random.Range(minEnemies * wave * waveMod* playerScale, maxEnemies * wave * waveMod* playerScale);
        enemiesSpawned = 0;

        isWave = true;
        allEnemiesDead = false;

        StartCoroutine(spawnEnemy(numEnemies));

        //Debug.Log("Starting wave, num enemies: " + numEnemies);
    }

    IEnumerator spawnEnemy(int numEnemies)
    {
        if (enemiesSpawned >= numEnemies)
        {
            //Debug.Log("Wave finished");
            isWave = false;
            yield break;
        }

        if (livingEnemies > 0)
            allEnemiesDead = false;


        float spawnRate = Random.Range(rate - 1f, rate + 1f);
        yield return new WaitForSeconds(spawnRate);

        foreach (var spawnPoint in spawnPoints)
        {
            GameObject en = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
            //speed base = 2.5 health basse = 70
            float waveFac =Mathf.Max (-Mathf.Log10((wave + 1) * 0.5f) + 3 , 0); // integral of wavefac up till wave 30 is 35
            en.GetComponent<EnemyAI>().speed += 0.07f * waveFac;
            en.GetComponent<Health>().maxHealth += 1.2f * waveFac;
            en.GetComponent<Health>().health = en.GetComponent<Health>().maxHealth;
            // TODO set enemy stats 
            enemiesSpawned++;
            livingEnemies++;
        }

        StartCoroutine(spawnEnemy(numEnemies));
    }

    public void onEnemyKilled()
    {
        livingEnemies--;
        if (livingEnemies <= 0)
            allEnemiesDead = true;
    }
}