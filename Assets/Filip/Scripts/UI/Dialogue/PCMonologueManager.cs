using Cinemachine;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class PCMonologueManager : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 6f;
    [SerializeField] private InputActionReference skipButton;

    [SerializeField] private DialogueTextController textController;


    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();

    [SerializeField] private string hint = "Použil lampièku pro hledání zrcátek.";

    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneFinished;

    public void Start()
    {
        ActivateLunarDudesCutscene(true);
    }


    public void ActivateLunarDudesCutscene(bool run)
    {
        OnCutsceneStart?.Invoke();
        if (run)
        {

            StartCoroutine(RunDialogueCoroutine(firstDialogue));
        }
        OnCutsceneFinished?.Invoke();
    }

    private IEnumerator RunDialogueCoroutine(List<DialogueLine> lines)
    {
        //turn off player

        textController.FadeInText();
        //Initial dialogue 

        for (int i = 0; i < lines.Count; i++)
        {
            MakeCharacterTalk(lines[i]);

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
        //    rotatingCamera.Priority = 0;

        // yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        textController.FadeOutText();
        //next scene
        textController.ShowTip(hint);

        Debug.Log("next scene");
    }

    private IEnumerator WaitForTimeOrSkip(float time)
    {
        float timer = 0f;

        while (skipButton.action.IsPressed())
            yield return null;

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
        //  if (character == null) return; make sure it doesnt call animation on player character

        textController.UpdateTextInBox(line.lineText, line.speakerName);

    }
}
