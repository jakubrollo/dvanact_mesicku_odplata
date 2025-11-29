using UnityEngine;
using System.Collections; // <--- REQUIRED for Coroutines (Waiting)

public class SceneTrigger : MonoBehaviour
{
    [Header("Cutscene Settings")]

    [Header("Scene References")]
    [Tooltip("Drag the Enemy object here so we can turn him off")]
    [SerializeField] private GameObject enemyObject;
    [SerializeField] private float fleeDuration = 3.0f;
    private bool hasTriggered = false; // Prevents triggering twice

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (GameProgressionManager.Instance != null)
            {
                hasTriggered = true; // Lock it so it doesn't fire multiple times
                StartCoroutine(PlayCutsceneSequence(other.gameObject));
            }
            else
            {
                Debug.LogError("No GameProgressionManager found! Start from the Menu scene.");
            }
        }
    }

    // This block handles the timing
    IEnumerator PlayCutsceneSequence(GameObject player)
    {
        Debug.Log("Starting 12 Months Cutscene...");

        // 1. Tell the Enemy to Run Away
        if (enemyObject != null)
        {
            // Get the AI script
            HorrorEnemyAI ai = enemyObject.GetComponent<HorrorEnemyAI>();
            if (ai != null)
            {
                ai.TriggerFlee(); // <--- Calls the new function we made
            }
        }

        yield return new WaitForSeconds(fleeDuration);

        if (enemyObject != null)
        {
            enemyObject.SetActive(false);
        }

        // 2. Stop the Player from moving (Optional, but feels better)
        // Try to find the movement script and disable it
        MonoBehaviour playerScript = player.GetComponent("SimplePlayerController") as MonoBehaviour;
        if (playerScript != null) playerScript.enabled = false;

        // 3. Turn ON the Cutscene (The 12 Months models/lights/camera)


        // 4. Load the Next Level
        Debug.Log("Cutscene finished. Loading next level.");
        //GameProgressionManager.Instance.LoadNextLevel();
    }
}