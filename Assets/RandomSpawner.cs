using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject trashcan;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 randomSpawnPos = new Vector3(Random.Range(-5, 6), Random.Range(-5, 6), 0);
            Instantiate(trashcan, randomSpawnPos, Quaternion.identity);
        }
    }
}
