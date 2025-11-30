using UnityEngine;
using UnityEngine.AI;

public class SceneInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemy; // Can be null (e.g. inside Cabin)
    [SerializeField] private AudioSource curAmbientAudioSource;
    [SerializeField] private AudioSource curMusicAudioSource;

    [Header("Configuration Lists")]
    [Tooltip("List of all possible places the Player can start")]
    [SerializeField] private Transform[] playerSpawnPoints;

    [Tooltip("List of all possible places the Enemy can start")]
    [SerializeField] private Transform[] enemySpawnPoints;

    [Tooltip("List of all Exits/Events. Only ONE will be active.")]
    [SerializeField] private GameObject[] eventsAndExits;

    void Start()
    {
        // 1. Get Data from Manager
        if (GameProgressionManager.Instance != null)
        {
            var data = GameProgressionManager.Instance.GetCurrentLevelData();
            SetupScene(data);
            AmbientClipsManager ambientManager =GameProgressionManager.Instance.GetComponentInChildren<AmbientClipsManager>();
            if(ambientManager != null)
            {
                ambientManager.AmbientAudioSource = curAmbientAudioSource;
                ambientManager.MusicAudioSource = curMusicAudioSource;
            }

        }
        else
        {
            Debug.LogWarning("No Manager found. Running scene with default inspector settings.");
            // Optional: You could call SetupScene with default data here for testing
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
        // --- 1. Setup Player Position ---
        if (player != null && playerSpawnPoints.Length > 0)
        {
            int index = Mathf.Clamp(data.spawnPointIndex, 0, playerSpawnPoints.Length - 1);

            // Disable CharacterController briefly to allow teleport
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            player.transform.position = playerSpawnPoints[index].position;
            player.transform.rotation = playerSpawnPoints[index].rotation;

            if (cc) cc.enabled = true;
        }

        // --- 2. Setup Exits / Events ---
        // First, turn EVERYTHING off
        foreach (var obj in eventsAndExits)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Then turn the specific one ON
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
                    ai.SetDifficulty(data.chaseSpeed, data.noticeBuildUpTime, data.maxDetectionRadius);
                }
            }
            else
            {
                enemy.SetActive(false);
            }
        }
    }
}