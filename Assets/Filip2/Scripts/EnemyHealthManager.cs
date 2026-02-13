using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthManager : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Hit Reaction")]
    [SerializeField] private float stunDuration = 0.5f;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private AudioSource audioSource;
    private NavMeshAgent agent;
    public bool isDead = false;

    private HorrorEnemyAI enemyController;
    private EnemySpawner spawner;

    [Header("Hit Tween")]
    [SerializeField] private float hitShakeDistance = 0.15f;
    [SerializeField] private float hitShakeDuration = 0.15f;

    [Header("Death Tween")]
    [SerializeField] private float deathSinkDepth = 2.5f;
    [SerializeField] private float deathDuration = 1.5f;
    [SerializeField] private float deathRotateSpeed = 360f;



    private void Start()
    {
        currentHealth = maxHealth;
 
        agent = GetComponent<NavMeshAgent>();
        enemyController = GetComponent<HorrorEnemyAI>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        audioSource.volume = 6f;
    }
    public void TakeDamage(int damage)
    {
        if(isDead) return;
        if(spawner == null)
        {
            spawner = enemyController.spawner;
        }
        Debug.Log("Bubak hit");

        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}");


        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);


        if (agent != null)
            StartCoroutine(StunAgent());

        StartCoroutine(HitShake());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitShake()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < hitShakeDuration)
        {
            elapsed += Time.deltaTime;
            float offset = Mathf.Sin(elapsed * 80f) * hitShakeDistance;
            transform.position = originalPos + transform.right * offset;
            yield return null;
        }

        transform.position = originalPos;
    }


    IEnumerator StunAgent()
    {
        if (agent == null) yield break;
        if(isDead) yield break;
        agent.isStopped = true;
        yield return new WaitForSeconds(stunDuration);
        if (isDead) yield break;
        agent.isStopped = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (agent != null)
            agent.enabled = false;

        // vypnout kolize (aby neblokoval hráèe)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        StartCoroutine(DeathAnimation());
    }


    
    IEnumerator DeathAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - Vector3.up * deathSinkDepth;

        float elapsed = 0f;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathDuration;

            // sink
            transform.position = Vector3.Lerp(startPos, endPos, t);

            // rotate
            transform.Rotate(Vector3.up, deathRotateSpeed * Time.deltaTime);

            yield return null;
        }

        // notify spawner
        if (spawner == null)
            spawner = enemyController.spawner;

        if (spawner != null)
            spawner.OnEnemyKilled(gameObject);
        else
        {
            if (spawner == null)
            {
                spawner = enemyController.spawner;
            }

            spawner.OnEnemyKilled(gameObject);
        }
    }

}
