using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Required for IEnumerator
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Photon.Pun; // Required for Multiplayer
using StarterAssets;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class HorrorEnemyAI : MonoBehaviourPun // Changed to MonoBehaviourPun
{
    // --- Configuration ---
    [Header("References")]
    // In multiplayer, we find the target dynamically
    [SerializeField] private Transform currentTarget;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float rotateToPlayerSpeed = 5.0f;

    [Header("Kill Settings")]
    [SerializeField] private float killDistance = 1.2f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip breathingSound;
    [SerializeField] private AudioClip crunchSound;

    [Header("Audio Settings")]
    [Range(0.1f, 3f)][SerializeField] private float crunchVolume = 1.0f;

    // --- State Variables ---
    private NavMeshAgent agent;
    private AudioSource mainAudioSource;
    private AudioSource breathingAudioSource;
    private bool isKilling = false;

    // --- SETUP & START ---

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        mainAudioSource = GetComponent<AudioSource>();

        // Ensure Main Audio is 3D
        mainAudioSource.spatialBlend = 1.0f;

        // Setup Breathing Audio (Local 3D sound)
        if (breathingSound != null)
        {
            breathingAudioSource = gameObject.AddComponent<AudioSource>();
            breathingAudioSource.clip = breathingSound;
            breathingAudioSource.loop = true;
            breathingAudioSource.spatialBlend = 1.0f; // Make it 3D so players hear it coming
            breathingAudioSource.rolloffMode = AudioRolloffMode.Linear;
            breathingAudioSource.maxDistance = 15f;
            breathingAudioSource.Play();
        }

        // --- MULTIPLAYER LOGIC ---
        // Only the Master Client calculates AI. 
        // Everyone else just syncs position via PhotonTransformView.
        if (!PhotonNetwork.IsMasterClient)
        {
            if (agent) agent.enabled = false;
        }
        else
        {
            if (agent)
            {
                agent.enabled = true;
                agent.speed = chaseSpeed;
            }
        }
    }

    // --- COMPATIBILITY METHODS ---

    // Called by EnemySpawner to set speed
    public void SetDifficulty(float newChaseSpeed)
    {
        this.chaseSpeed = newChaseSpeed;
        if (agent != null && PhotonNetwork.IsMasterClient)
        {
            agent.speed = chaseSpeed;
        }
    }

    // Kept for compatibility with your Spawner script, but logic is simplified for MP
    public void ConfigEnemy(GameObject player, EnemySpawner spawner)
    {
        // In multiplayer, we don't lock onto one specific player at spawn.
        // The Update loop finds the closest player dynamically.
    }

    // --- MAIN LOOP ---

    void Update()
    {
        // If we are NOT the Master Client, do nothing. 
        // Position is synced automatically via PhotonTransformView.
        if (!PhotonNetwork.IsMasterClient) return;

        if (isKilling) return;

        // 1. Find Target
        FindClosestPlayer();

        // 2. Move Agent
        if (currentTarget != null && agent.enabled)
        {
            agent.SetDestination(currentTarget.position);

            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (dist < killDistance)
            {
                KillTargetPlayer();
            }
        }
    }

    void FindClosestPlayer()
    {
        // Find all active players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (p == null) continue;

            // Optional: Check if player is already dead (if you have a script for that)
            // EnemyHealthManager health = p.GetComponent<EnemyHealthManager>();
            // if (health != null && health.isDead) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p.transform;
            }
        }

        currentTarget = closest;
    }

    // --- KILL LOGIC ---

    void KillTargetPlayer()
    {
        if (isKilling || currentTarget == null) return;

        isKilling = true;
        if (agent.enabled) agent.isStopped = true;

        // 1. Play Sound for Everyone
        photonView.RPC("RPC_PlayKillSound", RpcTarget.All);

        // 2. Tell the specific player to Die
        PhotonView targetView = currentTarget.GetComponent<PhotonView>();
        if (targetView != null)
        {
            // This calls the [PunRPC] Die() method on the PLAYER'S script (e.g., EnemyHealthManager or PlayerController)
            targetView.RPC("Die", targetView.Owner);
        }

        // 3. Destroy this enemy after a delay
        StartCoroutine(DestroyAfterKill());
    }

    [PunRPC]
    public void RPC_PlayKillSound()
    {
        if (crunchSound != null && mainAudioSource != null)
        {
            mainAudioSource.PlayOneShot(crunchSound, crunchVolume);
        }
    }

    IEnumerator DestroyAfterKill()
    {
        yield return new WaitForSeconds(3.0f);

        // Only Master Client can destroy networked objects
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}