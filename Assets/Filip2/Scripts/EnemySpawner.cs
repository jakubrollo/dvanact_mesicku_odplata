using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun; // Dùleité pro sí

public class EnemySpawner : MonoBehaviourPun
{
    [Header("References")]
    [Tooltip("Jméno prefabu ve sloce Resources (musí tam bıt string, ne pøetaenı objekt)")]
    [SerializeField] private string enemyPrefabName = "beastModel";

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private LayerMask groundLayer;

    // Seznamy u nepotøebujeme synchronizovat sloitì, staèí lokální check
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    [Header("Limits")]
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float minDistanceBetweenEnemies = 3f;
    [SerializeField] private int spawnAttempts = 10;

    private void Start()
    {
        // Spawner spustíme jen pokud toto je MÙJ hráè A zároveò jsem MASTER CLIENT
        // Ostatní hráèi nebudou spawnovat nic, jen se dívat.
        if (photonView.IsMine && PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnEnemiesInInterval());
        }
    }

    // Tuto metodu mùe volat HorrorEnemyAI, kdy umøe
    public void RemoveEnemyFromList(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
        }
    }

    IEnumerator SpawnEnemiesInInterval()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            // Vyèistíme seznam od null (znièenıch) nepøátel
            spawnedEnemies.RemoveAll(item => item == null);

            SpawnEnemyLogic();
        }
    }

    private void SpawnEnemyLogic()
    {
        // 1. Kontrola limitù
        if (spawnedEnemies.Count >= maxEnemies) return;

        // 2. Najdeme všechny hráèe ve scénì (abychom spawnovali poblí nìkoho)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        // Vybereme náhodného hráèe jako støed spawnu
        Transform randomPlayer = players[Random.Range(0, players.Length)].transform;

        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = randomPlayer.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Raycast na zem
            Ray ray = new Ray(spawnPos + Vector3.up * raycastHeight, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                Vector3 groundPos = hit.point;

                if (NavMesh.SamplePosition(groundPos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    Vector3 finalPos = navHit.position;

                    // Kontrola vzdálenosti od ostatních nepøátel
                    bool tooClose = false;
                    foreach (var e in spawnedEnemies)
                    {
                        if (e != null && Vector3.Distance(finalPos, e.transform.position) < minDistanceBetweenEnemies)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    // --- SÍOVİ SPAWN ---
                    // Místo Instantiate pouijeme PhotonNetwork.Instantiate
                    GameObject newEnemy = PhotonNetwork.Instantiate(enemyPrefabName, finalPos, Quaternion.identity);

                    spawnedEnemies.Add(newEnemy);
                    return;
                }
            }
        }
    }
}