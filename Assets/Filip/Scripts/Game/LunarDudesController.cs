using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum LunarDudesStage
{
    First,
    Second,
    Third
}

public class LunarDudesController : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;
    [SerializeField] private InputActionReference skipButton;

    [SerializeField] private DialogueTextController textController;
    [SerializeField] private CinemachineVirtualCamera rotatingCamera;

    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();
    [SerializeField] private List<DialogueLine> secondDialogue = new List<DialogueLine>();
    [SerializeField] private List<DialogueLine> thirdDialogue = new List<DialogueLine>();
    [SerializeField] private HorrorEnemyAI enemy;

    public UnityEvent OnLunarCutsceneStart;
    public UnityEvent OnLunarCutsceneFinished;

    [SerializeField] LunarDudesStage stage = LunarDudesStage.First;
    [SerializeField] private GameObject player;

    public void Start()
    {
        // Uncomment this if you want it to start automatically, 
        // otherwise call ActivateLunarDudesCutscene from a Trigger
        // ActivateLunarDudesCutscene(stage);
    }

    public void ActivateLunarDudesCutscene(LunarDudesStage stage)
    {
        OnLunarCutsceneStart?.Invoke();

        if (stage == LunarDudesStage.First)
        {
            StartCoroutine(RunDialogueCoroutine(firstDialogue));
        }
        else if (stage == LunarDudesStage.Second)
        {
            StartCoroutine(RunDialogueCoroutine(secondDialogue));
        }
        else if (stage == LunarDudesStage.Third)
        {
            StartCoroutine(RunDialogueCoroutine(thirdDialogue));
        }
    }

    private IEnumerator RunDialogueCoroutine(List<DialogueLine> lines)
    {
        //get enemies to leave
        if(enemy != null)
          //  enemy.TriggerFlee();
        if (player != null)
        {
            var cc = player.GetComponent<UnityEngine.CharacterController>();
            if (cc) cc.enabled = false;
            var moveScript = player.GetComponent<FirstPersonController>();
            if (moveScript) moveScript.enabled = false;
            var candleScript = player.GetComponentInChildren<CandleBehavior>();
            if (candleScript != null)
            {
                candleScript.gameObject.SetActive(false);
            }
        }
        // 1. Setup
        if (textController != null) textController.FadeInText();
        if (rotatingCamera != null) rotatingCamera.Priority = 300;

        // 2. Play Lines
        // BUG FIX: Changed 'firstDialogue' to 'lines' so it works for stage 2 and 3
        for (int i = 0; i < lines.Count; i++)
        {
            MakeCharacterTalk(lines[i]);

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }

        // 3. Cleanup
        if (textController != null) textController.FadeOutText();

        Debug.Log("Cutscene Finished. Triggering Next Level...");

        // 4. Fire Events
        OnLunarCutsceneFinished?.Invoke();

        // 5. --- CALL THE MANAGER HERE ---
        // This works because 'Instance' is static and lives in DontDestroyOnLoad
        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("No GameProgressionManager found! Did you start from the Menu?");
        }
    }

    private IEnumerator WaitForTimeOrSkip(float time)
    {
        float timer = 0f;

        // Wait if button is held down initially (prevent accidental skips)
        if (skipButton != null)
            while (skipButton.action.IsPressed()) yield return null;

        while (timer < time)
        {
            timer += Time.deltaTime;

            if (skipButton != null && skipButton.action != null && skipButton.action.WasPressedThisFrame())
            {
                break;
            }

            yield return null;
        }
    }

    private void MakeCharacterTalk(DialogueLine line)
    {
        if (textController != null)
            textController.UpdateTextInBox(line.lineText, line.speakerName);
    }
}