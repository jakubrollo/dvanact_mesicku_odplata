using UnityEngine;
using UnityEngine.AI;
using Photon.Pun; // 1. Added Photon Namespace

public class SceneInitializer : MonoBehaviour
{
    [Header("Multiplayer Setup")]
    [SerializeField] private string playerPrefabName = "PlayerCapsule"; // Name of your prefab in Resources

    [Header("References")]
    // [SerializeField] private GameObject player; // REMOVED: We don't use the scene player anymore
    [SerializeField] private GameObject enemy; 
    [SerializeField] private AudioSource curAmbientAudioSource;
    [SerializeField] private AudioSource curMusicAudioSource;
    [SerializeField] private AudioSource curWalkAudioSource;

    [Header("Configuration Lists")]
    [Tooltip("List of all possible places the Player can start")]
    [SerializeField] private Transform[] playerSpawnPoints;

    [Tooltip("List of all possible places the Enemy can start")]
    [SerializeField] private Transform[] enemySpawnPoints;

    [Tooltip("List of all Exits/Events. Only ONE will be active.")]
    [SerializeField] private GameObject[] eventsAndExits;

    [SerializeField] private PCMonologueManager pcMonolog;

    void Start()
    {
        // 1. Get Data from Manager
        if (GameProgressionManager.Instance != null)
        {
            var data = GameProgressionManager.Instance.GetCurrentLevelData();

            if (data.ForestStage == LunarDudesStage.First)
            {
               /* if(pcMonolog != null)
                    pcMonolog.ActivatePCMonologue(true);*/
            }

            SetupScene(data);

            // Audio Setup for the Ambient Manager (Scene based)
            AmbientClipsManager ambientManager = GameProgressionManager.Instance.GetComponentInChildren<AmbientClipsManager>();
            if (ambientManager != null)
            {
                ambientManager.AmbientAudioSource = curAmbientAudioSource;
                ambientManager.MusicAudioSource = curMusicAudioSource;
                // Note: WalkingAudioSource is assigned inside SetupScene after instantiation
            }
        }
        else
        {
            Debug.LogWarning("No Manager found. Running scene with default inspector settings.");
        }
    }

    public void LoadNextLevel()
    {
        if (GameProgressionManager.Instance != null)
        {
            print("Loading next level via SceneInitializer...");
            GameProgressionManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogWarning("No Manager found. Cannot load next level.");
        }
    }

    void SetupScene(GameProgressionManager.LevelData data)
    {
        // --- 1. SPAWN PLAYER (Networked) ---
        if (PhotonNetwork.IsConnected)
        {
            // Pick the spawn point from the list based on LevelData
            int index = Mathf.Clamp(data.spawnPointIndex, 0, playerSpawnPoints.Length - 1);
            Transform spawnPoint = playerSpawnPoints[index];

            // Add random offset so players don't spawn inside each other
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 spawnPos = spawnPoint.position + randomOffset + (Vector3.up * 0.5f); // Slight lift to avoid floor clipping

            object[] myCustomData = new object[] { PhotonNetwork.NickName };

            // Instantiate the player
            GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnPoint.rotation, 0, myCustomData);

            // --- AUDIO CONNECTION (Important for Walking Sounds) ---
            AmbientClipsManager ambientManager = GameProgressionManager.Instance.GetComponentInChildren<AmbientClipsManager>();
            if (ambientManager != null && newPlayer != null)
            {
                // Find or Add AudioSource on the new player
                AudioSource playerAudio = newPlayer.GetComponent<AudioSource>();
                if (playerAudio == null) playerAudio = newPlayer.AddComponent<AudioSource>();
                
                // Connect it to the manager
                ambientManager.WalkingAudioSource = playerAudio;
            }
        }
        else
        {
            Debug.LogWarning("Not connected to Photon! Cannot spawn player.");
        }

        // --- 2. Setup Exits / Events ---
        foreach (var obj in eventsAndExits)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (eventsAndExits.Length > 0)
        {
            int index = Mathf.Clamp(data.activeEventIndex, 0, eventsAndExits.Length - 1);
            if (eventsAndExits[index] != null)
                eventsAndExits[index].SetActive(true);
        }

        // --- 3. Setup Enemy ---
        if (enemy != null)
        {
            if (data.hasEnemy)
            {
                enemy.SetActive(true);

                // Position Enemy
                if (enemySpawnPoints.Length > 0)
                {
                    int spawnIndex = Mathf.Clamp(data.enemySpawnPointIndex, 0, enemySpawnPoints.Length - 1);
                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                    if (agent) agent.Warp(enemySpawnPoints[spawnIndex].position);
                    else enemy.transform.position = enemySpawnPoints[spawnIndex].position;
                }

                // Configure Enemy Stats
                HorrorEnemyAI ai = enemy.GetComponent<HorrorEnemyAI>();
                if (ai != null)
                {
                    ai.SetDifficulty(data.chaseSpeed);
                }
            }
            else
            {
                enemy.SetActive(false);
            }
        }
    }
}