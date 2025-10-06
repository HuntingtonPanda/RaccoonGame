using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject trashcan;

    // World boundaries
    const int xMin = -10;
    const int xMax = 10;
    const int yMin = -7;
    const int yMax = 7;

    // Constants for control
    const int NUM_TRASHCANS = 10;               // How many trashcans to spawn
    const float TRASHCAN_SIZE = 1.0f;           // Size of each trashcan
    const float MIN_DISTANCE_MULTIPLIER = 4f; // Spacing factor (buffer between cans)

    List<Vector2> spawnedPositions = new List<Vector2>();

    private void Start()
    {
        SpawnTrashcans();
    }

    private void SpawnTrashcans()
    {
        int spawned = 0;
        int attempts = 0;
        const int MAX_ATTEMPTS = 1000; // Just in case

        while (spawned < NUM_TRASHCANS && attempts < MAX_ATTEMPTS)
        {
            attempts++;

            Vector2 randomSpawnPos = new Vector2(Random.Range(xMin, xMax + 1), Random.Range(yMin, yMax + 1));

            if (IsPositionValid(randomSpawnPos))
            {
                Instantiate(trashcan, randomSpawnPos, Quaternion.identity);
                spawnedPositions.Add(randomSpawnPos);
                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned} trashcans after {attempts} attempts.");
    }

    private bool IsPositionValid(Vector2 candidate)
    {
        float minDistance = TRASHCAN_SIZE * MIN_DISTANCE_MULTIPLIER;

        foreach (Vector2 existing in spawnedPositions)
        {
            if (Vector2.Distance(candidate, existing) < minDistance)
                return false;
        }

        return true;
    }
}
