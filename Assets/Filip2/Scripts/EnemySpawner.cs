using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private LayerMask groundLayer;

    public List<Transform> players = new List<Transform>();
    private List<GameObject> spawnedEnemies = new List<GameObject>();

   // public List<Transform> allPlayers = new List<Transform> ();

    [Header("Limits")]
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float minDistanceBetweenEnemies = 3f;
    [SerializeField] private int spawnAttempts = 10;

    private void Start()
    {
        StartCoroutine(SpawnEnemiesInInterval());
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
            Destroy(enemy);
        }
        else
        {
            Debug.LogError("removing enemy that isnt in the list, wtf");
        }
    }

    IEnumerator SpawnEnemiesInInterval()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (players.Count == 0) return;
        if (spawnedEnemies.Count >= maxEnemies) return;

        Transform randomPlayer = players[Random.Range(0, players.Count)];

        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = randomPlayer.position +
                               new Vector3(randomCircle.x, 0, randomCircle.y);

            Ray ray = new Ray(spawnPos + Vector3.up * raycastHeight, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                Vector3 groundPos = hit.point;

                if (NavMesh.SamplePosition(groundPos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    Vector3 finalPos = navHit.position;

                    bool tooClose = false;
                    foreach (var e in spawnedEnemies)
                    {
                        if (e == null) continue;

                        if (Vector3.Distance(finalPos, e.transform.position) < minDistanceBetweenEnemies)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    GameObject enemy = Instantiate(enemyPrefab, finalPos, Quaternion.identity);

                 /*   var allPlayers = GameObject
                        .FindGameObjectsWithTag("Player")
                        .Select(go => go.transform)
                        .ToList();*/

                    enemy.GetComponent<HorrorEnemyAI>()
                         .ConfigEnemy(transform.parent.gameObject, this);

                    spawnedEnemies.Add(enemy);
                    return; 
                }
            }
        }

        // Debug when no valid spawn found
        Debug.Log("No valid spawn position found.");
    }
}
