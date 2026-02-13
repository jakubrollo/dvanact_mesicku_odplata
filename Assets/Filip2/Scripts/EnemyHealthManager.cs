using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun; // 1. Pøidán Photon

// 2. Dìdíme od MonoBehaviourPun pro pøístup k síti
public class EnemyHealthManager : MonoBehaviourPun, IDamageable
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

    // Odkaz na Controller (pro vypnutí pohybu), ale spawner už nepotøebujeme
    private HorrorEnemyAI enemyController;

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
        audioSource.spatialBlend = 1f; // 3D zvuk
        audioSource.playOnAwake = false;
        audioSource.volume = 1f; // 6f je moc, Unity to stejnì oøízne na 1 nebo to bude zkreslené
    }

    // Tuto metodu volá GunController
    public void TakeDamage(int damage)
    {
        // Místo pøímého ubrání života pošleme zprávu všem (vèetnì sebe)
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        // Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}");

        // Pøehrát zvuk zásahu
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        // Shake efekt (pouze vizuální, lokální)
        StartCoroutine(HitShake());

        // Stun (pokud jsme MasterClient, protože ten ovládá NavMeshAgenta)
        if (PhotonNetwork.IsMasterClient && agent != null && agent.enabled)
        {
            StartCoroutine(StunAgent());
        }

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
            // Hýbeme jen modelem, ne celým transformem, pokud je agent aktivní,
            // ale pro jednoduchost to necháme, agent to pak srovná.
            transform.position = originalPos + transform.right * offset;
            yield return null;
        }
        // Vrátíme se na pozici (nebo necháme agenta)
    }

    IEnumerator StunAgent()
    {
        if (agent == null) yield break;
        if (isDead) yield break;

        if (agent.isActiveAndEnabled)
            agent.isStopped = true;

        yield return new WaitForSeconds(stunDuration);

        if (isDead) yield break;

        if (agent.isActiveAndEnabled)
            agent.isStopped = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        // Vypnout agenta (jen MasterClient to mùže udìlat efektivnì)
        if (agent != null) agent.enabled = false;

        // Vypnout kolize
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

            // Sink & Rotate (dìje se lokálnì na každém PC pro vizuální efekt)
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.Rotate(Vector3.up, deathRotateSpeed * Time.deltaTime);

            yield return null;
        }

        // --- SÍOVÉ ZNIÈENÍ ---
        // Jen Master Client má právo znièit síový objekt.
        // Ostatním zmizí automaticky, jakmile ho Master znièí.
        // Spawner (ten nový) automaticky pozná, že je objekt pryè (je null) a vyhodí ho ze seznamu.
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}