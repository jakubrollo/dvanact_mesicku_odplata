using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Cinemachine;
using System.Linq;
using System.Collections.Generic;
using StarterAssets;
using static UnityEditor.Experimental.GraphView.GraphView;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class HorrorEnemyAI : MonoBehaviour
{
    // --- Configuration ---
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("The Main Camera (The one with the Cinemachine Brain)")]
    [SerializeField] private Transform playerCameraTransform;

    [Tooltip("Drag your 'PlayerFollowCamera' (the Cinemachine Virtual Camera object) here")]
    [SerializeField] private GameObject playerVirtualCameraObject; // <--- NEW REFERENCE
    [SerializeField] private CinemachineVirtualCamera deathCameraObject;

    [Tooltip("Drag the Player's movement script here so we can disable it on death")]
    [SerializeField] private MonoBehaviour playerMovementScript;


    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float rotateToPlayerSpeed = 5.0f;

    [Header("Kill Settings")]
    [SerializeField] private float killDistance = 1.2f;
    [SerializeField] private float enemyEyeHeight = 1.4f; // Adjusted for typical player height
    [SerializeField] private AudioClip crunchSound;
    [Range(0.1f, 3f)][SerializeField] private float crunchVolume = 2.0f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip breathingSound;
    [SerializeField] private AudioClip stingerSound;
    [SerializeField] private AudioClip chaseGrowlSound;

    [Header("Audio Settings")]
    [SerializeField] private AnimationCurve breathingVolumeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0.1f, 3f)][SerializeField] private float stingerVolume = 1.0f;
    [Range(0.1f, 1.5f)][SerializeField] private float chaseVolume = 1.0f;
    [SerializeField] private float audioFadeDuration = 0.5f;

    [Header("Breathing Speed")]
    [SerializeField] private float minBreathingPitch = 1.0f;
    [SerializeField] private float maxBreathingPitch = 1.8f;

    // --- State Variables ---
    [HideInInspector] public NavMeshAgent agent;
    private AudioSource mainAudioSource;
    private AudioSource breathingAudioSource;

    private Transform killCamLookTarget;
    public bool cameraIsLocked = false;

 //   [SerializeField] public List<Transform> players = new List<Transform>();

    private Transform currentTarget;

    private bool isKilling = false;

    private float pathCheckTimer = 0f;
    [SerializeField] private float pathCheckInterval = 1.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.2f;

    private Vector3 lastPosition;

    [HideInInspector] public EnemySpawner spawner;

    private EnemyHealthManager healthManager;

    void CheckPathValidity()
    {
        pathCheckTimer += Time.deltaTime;

        if (pathCheckTimer < pathCheckInterval) return;

        pathCheckTimer = 0f;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning("Enemy has no valid path ? destroying");
            Destroy(gameObject);
            return;
        }

        float movedDistance = Vector3.Distance(transform.position, lastPosition);

        if (movedDistance < stuckDistanceThreshold && agent.remainingDistance > 1f)
        {
            Debug.LogWarning("Enemy is stuck ? destroying");
            spawner.OnEnemyKilled(gameObject);
           // Destroy(gameObject);
            return;
        }

        lastPosition = transform.position;
    }


    public void ConfigEnemy(GameObject playerParent, EnemySpawner spawner)
    {
       // this.players = players;
        this.spawner = spawner;


        GameObject player = playerParent.transform.Find("PlayerCapsule").gameObject;

        currentTarget = player.transform;

        playerTransform = player.transform;
        
        playerCameraTransform = playerParent.transform.Find("MainCamera");

        playerVirtualCameraObject = playerParent.transform.Find("PlayerFollowCamera").gameObject; 
        deathCameraObject = playerParent.transform.Find("DeathCamera").gameObject.GetComponent<CinemachineVirtualCamera>();

        playerMovementScript = player.GetComponentInChildren<FirstPersonController>();//Extremly horrible
    }
    void Start()
    {
        healthManager = GetComponent<EnemyHealthManager>();
        agent = GetComponent<NavMeshAgent>();

        mainAudioSource = GetComponent<AudioSource>();

        mainAudioSource.volume = 1.0f;
        mainAudioSource.spatialBlend = 1.0f;
        mainAudioSource.loop = false;

        breathingAudioSource = gameObject.AddComponent<AudioSource>();
        breathingAudioSource.clip = breathingSound;
        breathingAudioSource.loop = true;
        breathingAudioSource.spatialBlend = 0.0f;
        breathingAudioSource.playOnAwake = true;
        breathingAudioSource.Play();
    }

    public void TriggerFlee()
    {
        Debug.LogWarning("enemy should flee");
    }

    void Update()
    {
        if (healthManager.isDead) return;
        if (isKilling) return;

        if (!spawner.isActiveAndEnabled)
        {
            PickClosestPlayer();
        }

       /* if (currentTarget == null)
        {
            PickClosestPlayer();
            return;
        }*/

        agent.SetDestination(currentTarget.position);

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        if (dist < killDistance)
        {
            KillPlayer(currentTarget);
        }
    }

    void KillPlayer(Transform player)
    {
        if (isKilling) return;

        isKilling = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // disable player movement
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // play crunch
        if (crunchSound != null)
        {
            mainAudioSource.Stop();
            mainAudioSource.loop = false;
            mainAudioSource.clip = crunchSound;
            mainAudioSource.volume = crunchVolume;
            mainAudioSource.Play();
        }

        HandleKillCameraLock();

        StartCoroutine(DeathSequence());
    

        //PickClosestPlayer();
    }


    void HandleKillCameraLock()
    {
        if (cameraIsLocked) return;
        if (playerVirtualCameraObject != null && deathCameraObject != null)
        {
            cameraIsLocked = true;
            var vcam = playerVirtualCameraObject.GetComponent<CinemachineVirtualCamera>();
            deathCameraObject.transform.position = playerCameraTransform.position;
            vcam.Priority = 0;
            deathCameraObject.Priority = 20;

            if (killCamLookTarget == null)
            {
                killCamLookTarget = new GameObject("KillCamLookTarget").transform;
            }

            killCamLookTarget.position = transform.position + Vector3.up * enemyEyeHeight;
            deathCameraObject.LookAt = killCamLookTarget;
        }
        PickClosestPlayer();
    }


    public void SetDifficulty(float newChaseSpeed)
    {
        this.chaseSpeed = newChaseSpeed;

    }

    void HandleBreathingAudio()
    {
      /*  if (breathingAudioSource != null)
        {
            breathingAudioSource.volume = breathingVolumeCurve.Evaluate(currentNoticeLevel);
            breathingAudioSource.pitch = Mathf.Lerp(minBreathingPitch, maxBreathingPitch, currentNoticeLevel);
        }*/
    }



    IEnumerator FadeOutAudio(float duration)
    {
        float startVol = mainAudioSource.volume;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        mainAudioSource.volume = 0f;
    }

    IEnumerator FadeOutAndStop(float duration)
    {
        float startVol = mainAudioSource.volume;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        mainAudioSource.Stop();
        mainAudioSource.volume = startVol;
    }
    /*
    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }
    */

    // --- NEW DEATH SEQUENCE COROUTINE ---
    IEnumerator DeathSequence()
    {
        // Wait for the crunch sound to play out a bit (e.g., 2 seconds)
        // Adjust this float to match your audio clip length
        yield return new WaitForSeconds(2.0f);

        // Tell the Manager to reload the current level
        if (GameProgressionManager.Instance != null)
        {
            var vcam = playerVirtualCameraObject.GetComponent<CinemachineVirtualCamera>();
            vcam.Priority = 15;
            deathCameraObject.Priority = 0;
            cameraIsLocked = false;
            GameProgressionManager.Instance.ReloadCurrentLevel();
        }
        else
        {
            Debug.LogError("No GameProgressionManager! Reloading scene manually.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }


    void PickClosestPlayer()
    {

          List<Transform>  players = GameObject.FindGameObjectsWithTag("Player")
                .Select(go => go.transform)
                .ToList();
        

        players.RemoveAll(p => p == null);

        if (players.Count == 0)
        {
            currentTarget = null;
            return;
        }

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(transform.position, p.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p;
            }
        }

        if(!spawner.isActiveAndEnabled)
        {
            spawner = closest.gameObject.GetComponent<EnemySpawner>();
        }

        currentTarget = closest;
        ConfigEnemy(closest.gameObject, spawner);
    }
}