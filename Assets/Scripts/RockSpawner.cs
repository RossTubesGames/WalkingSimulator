using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Rock Settings")]
    public GameObject rockPrefab;          // The rock prefab to spawn
    public Transform spawnPoint;           // The general top-of-mountain spawn position
    public float spawnRadius = 3f;         // How far from the spawnPoint rocks can appear
    public float spawnInterval = 2f;       // How often to spawn a new rock
    public float rockLifetime = 10f;       // How long before it despawns

    [Header("Spawn Control")]
    public int maxRocks = 5;               // Max rocks active at once

    private int activeRocks = 0;

    void Start()
    {
        StartCoroutine(SpawnRocksRoutine());
    }

    IEnumerator SpawnRocksRoutine()
    {
        while (true)
        {
            if (activeRocks < maxRocks)
            {
                SpawnRock();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRock()
    {
        // Pick a random position around the spawn point
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0f,
            Random.Range(-spawnRadius, spawnRadius)
        );

        Vector3 spawnPos = spawnPoint.position + randomOffset;

        GameObject rock = Instantiate(rockPrefab, spawnPos, spawnPoint.rotation);
        activeRocks++;
        StartCoroutine(DespawnAfterTime(rock));
    }

    IEnumerator DespawnAfterTime(GameObject rock)
    {
        yield return new WaitForSeconds(rockLifetime);
        Destroy(rock);
        activeRocks--;
    }
}
