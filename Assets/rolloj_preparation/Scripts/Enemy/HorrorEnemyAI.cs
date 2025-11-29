using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class HorrorEnemyAI : MonoBehaviour
{
    // --- Configuration ---
    [Header("References")]
    [SerializeField] private CandleBehavior playerCandle;
    [SerializeField] private Transform playerTransform;

    [Tooltip("The Main Camera (The one with the Cinemachine Brain)")]
    [SerializeField] private Transform playerCameraTransform;

    [Tooltip("Drag your 'PlayerFollowCamera' (the Cinemachine Virtual Camera object) here")]
    [SerializeField] private GameObject playerVirtualCameraObject; // <--- NEW REFERENCE

    [Tooltip("Drag the Player's movement script here so we can disable it on death")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Notice Logic")]
    [SerializeField] private float noticeBuildUpTime = 5f;
    [SerializeField] private float noticeDecayTime = 2f;
    [SerializeField] private AnimationCurve noticeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Detection Radius (Independent)")]
    [SerializeField] private float minRadius = 3f;
    [SerializeField] private float maxRadius = 15f;
    [SerializeField] private float radiusGrowthSpeed = 2.0f;
    [SerializeField] private float radiusDecaySpeed = 5.0f;
    [SerializeField] private float chaseThreshold = 0.9f;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float fleeSpeed = 7.0f;
    [SerializeField] private float wanderRadius = 10f;
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
    private NavMeshAgent agent;
    private AudioSource mainAudioSource;
    private AudioSource breathingAudioSource;

    private float rawNoticeValue = 0f;
    private float currentNoticeLevel = 0f;
    private float currentDetectionRadius;
    private float chaseMemoryTimer = 0f;
    private float screamTimer = 0f;
    private Coroutine activeFadeCoroutine;
    private bool isForcedFlee = false;
    private Vector3 lastKnownPosition;
    public void TriggerFlee()
    {
        isForcedFlee = true;
        SetState(EnemyState.Disappear);
    }
    private enum EnemyState { Wander, NoticeReaction, Chase, Disappear, Kill }
    private EnemyState currentState = EnemyState.Wander;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        mainAudioSource = GetComponent<AudioSource>();

        mainAudioSource.volume = 1.0f;
        mainAudioSource.spatialBlend = 1.0f;
        mainAudioSource.loop = false;

        currentDetectionRadius = minRadius;

        breathingAudioSource = gameObject.AddComponent<AudioSource>();
        breathingAudioSource.clip = breathingSound;
        breathingAudioSource.loop = true;
        breathingAudioSource.spatialBlend = 0.0f;
        breathingAudioSource.playOnAwake = true;
        breathingAudioSource.Play();
        if (GameProgressionManager.Instance != null)
        {
            var data = GameProgressionManager.Instance.GetCurrentLevelData();
            if (data.hasEnemy)
            {
                this.chaseSpeed = data.chaseSpeed;
                this.noticeBuildUpTime = data.noticeBuildUpTime;
                this.maxRadius = data.maxDetectionRadius;
                this.currentDetectionRadius = minRadius;
            }
        }
        SetState(EnemyState.Wander);
    }

    void Update()
    {
        // If we are killing the player, skip logic and just lock camera
        if (currentState == EnemyState.Kill)
        {
            HandleKillCameraLock();
            return;
        }

        HandleNoticeLogic();
        HandleRadiusLogic();
        HandleBreathingAudio();
        HandleStateLogic();
    }

    // --- UPDATED: Works by taking control from Cinemachine ---
    void HandleKillCameraLock()
    {
        if (playerCameraTransform != null)
        {
            // Calculate where the enemy's eyes are
            Vector3 enemyEyes = transform.position + Vector3.up * enemyEyeHeight;

            // Determine direction from Camera to Enemy Eyes
            Vector3 direction = (enemyEyes - playerCameraTransform.position).normalized;

            // Smoothly rotate camera towards enemy
            Quaternion lookRot = Quaternion.LookRotation(direction);

            // Note: Since we disabled the Cinemachine Object, this manual rotation now works!
            playerCameraTransform.rotation = Quaternion.Slerp(playerCameraTransform.rotation, lookRot, Time.deltaTime * 8f);
        }
    }

    void HandleNoticeLogic()
    {
        if (playerCandle.candle_turned_on) // change this to Martin's candle, otherwise null
            rawNoticeValue += Time.deltaTime / noticeBuildUpTime;
        else
            rawNoticeValue -= Time.deltaTime / noticeDecayTime;

        rawNoticeValue = Mathf.Clamp01(rawNoticeValue);
        currentNoticeLevel = noticeCurve.Evaluate(rawNoticeValue);
    }
    public void SetDifficulty(float newChaseSpeed, float newNoticeTime, float newMaxRadius)
    {
        this.chaseSpeed = newChaseSpeed;
        this.noticeBuildUpTime = newNoticeTime;
        this.maxRadius = newMaxRadius;

        // Reset state to be safe
        currentDetectionRadius = minRadius;
    }
    void HandleRadiusLogic()
    {
        if (playerCandle.candle_turned_on)
            currentDetectionRadius += radiusGrowthSpeed * Time.deltaTime;
        else
            currentDetectionRadius -= radiusDecaySpeed * Time.deltaTime;

        currentDetectionRadius = Mathf.Clamp(currentDetectionRadius, minRadius, maxRadius);
    }

    void HandleBreathingAudio()
    {
        if (breathingAudioSource != null)
        {
            breathingAudioSource.volume = breathingVolumeCurve.Evaluate(currentNoticeLevel);
            breathingAudioSource.pitch = Mathf.Lerp(minBreathingPitch, maxBreathingPitch, currentNoticeLevel);
        }
    }

    void HandleStateLogic()
    {
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Wander:
                agent.speed = wanderSpeed;
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    agent.SetDestination(GetRandomNavMeshPoint(transform.position, wanderRadius));
                }

                if (currentNoticeLevel >= chaseThreshold || distToPlayer < currentDetectionRadius)
                {
                    SetState(EnemyState.NoticeReaction);
                }
                break;

            case EnemyState.NoticeReaction:
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                direction.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateToPlayerSpeed);

                lastKnownPosition = playerTransform.position;
                screamTimer -= Time.deltaTime;

                if (screamTimer <= audioFadeDuration && activeFadeCoroutine == null)
                {
                    activeFadeCoroutine = StartCoroutine(FadeOutAudio(audioFadeDuration));
                }

                if (screamTimer <= 0)
                {
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;

                // --- KILL TRIGGER ---
                if (distToPlayer < killDistance)
                {
                    SetState(EnemyState.Kill);
                    return;
                }

                if (playerCandle.candle_turned_on || distToPlayer < minRadius)
                {
                    lastKnownPosition = playerTransform.position;
                    chaseMemoryTimer = 2.0f;
                }
                else
                {
                    chaseMemoryTimer -= Time.deltaTime;
                }

                agent.SetDestination(lastKnownPosition);

                if (chaseMemoryTimer <= 0f)
                {
                    if (!agent.pathPending && agent.remainingDistance < 1.5f)
                    {
                        SetState(EnemyState.Disappear);
                    }
                }
                break;

            case EnemyState.Disappear:
                agent.speed = fleeSpeed;
                if (isForcedFlee)
                {
                    if (!agent.pathPending && agent.remainingDistance < 1f)
                    {
                        Vector3 fleePos = transform.position + transform.forward * 10f;
                        agent.SetDestination(GetRandomNavMeshPoint(fleePos, 5f));
                    }
                    return;
                }
                if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    SetState(EnemyState.Wander);
                }
                break;
        }
    }

    void SetState(EnemyState newState)
    {
        if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
        activeFadeCoroutine = null;

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Wander:
                agent.acceleration = 8f;
                mainAudioSource.Stop();
                break;

            case EnemyState.NoticeReaction:
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
                lastKnownPosition = playerTransform.position;

                mainAudioSource.Stop();
                mainAudioSource.loop = false;
                mainAudioSource.clip = stingerSound;
                mainAudioSource.volume = stingerVolume;
                mainAudioSource.Play();

                screamTimer = 2.5f;
                break;

            case EnemyState.Chase:
                agent.isStopped = false;
                agent.acceleration = 60f;

                mainAudioSource.Stop();
                mainAudioSource.clip = chaseGrowlSound;
                mainAudioSource.loop = true;
                mainAudioSource.volume = chaseVolume;
                mainAudioSource.Play();
                break;

            case EnemyState.Disappear:
                agent.acceleration = 60f;
                activeFadeCoroutine = StartCoroutine(FadeOutAndStop(1.0f));

                Vector3 fleeDir = (transform.position - playerTransform.position).normalized;
                Vector3 fleePos = transform.position + fleeDir * 20f;
                agent.SetDestination(GetRandomNavMeshPoint(fleePos, 5f));
                break;

            case EnemyState.Kill:
                // 1. Freeze Enemy
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                // 2. Disable Player Controls
                if (playerMovementScript != null) playerMovementScript.enabled = false;

                // 3. DISABLE CINEMACHINE (Crucial!)
                if (playerVirtualCameraObject != null) playerVirtualCameraObject.SetActive(false);

                // 4. Play Crunch Sound
                mainAudioSource.Stop();
                mainAudioSource.loop = false;
                mainAudioSource.clip = crunchSound;
                mainAudioSource.volume = crunchVolume;
                mainAudioSource.Play();

                if (breathingAudioSource) breathingAudioSource.Stop();
                StartCoroutine(DeathSequence());
                break;
        }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.Lerp(Color.green, Color.red, currentNoticeLevel);
        Gizmos.DrawWireSphere(transform.position, currentDetectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minRadius);

        if (currentState == EnemyState.Chase || currentState == EnemyState.NoticeReaction)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastKnownPosition, 1f);
            Gizmos.DrawLine(transform.position, lastKnownPosition);
        }
    }

    // --- NEW DEATH SEQUENCE COROUTINE ---
    IEnumerator DeathSequence()
    {
        // Wait for the crunch sound to play out a bit (e.g., 2 seconds)
        // Adjust this float to match your audio clip length
        yield return new WaitForSeconds(2.0f);

        // Tell the Manager to reload the current level
        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.ReloadCurrentLevel();
        }
        else
        {
            Debug.LogError("No GameProgressionManager! Reloading scene manually.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}