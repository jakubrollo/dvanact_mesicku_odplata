using Cinemachine;
using NUnit.Framework;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class IntoOutroCutsceneManager : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;
    [SerializeField] private InputActionReference skipButton;

    [SerializeField] private DialogueTextController textController;

    //   [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private CinemachineVirtualCamera rotatingCamera;

    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();
    [SerializeField] private List<DialogueLine> secondDialogue = new List<DialogueLine>();


    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneFinished;

    public enum Stage
    {
        First,
        Second
    }

    [SerializeField] Stage stage = Stage.First;

    public void Start()
    {
        ActivateLunarDudesCutscene(stage);
    }


    public void ActivateLunarDudesCutscene(Stage stage)
    {
        OnCutsceneStart?.Invoke();
        if (stage == Stage.First)
        {

            StartCoroutine(RunDialogueCoroutine(firstDialogue));
        }
        else if (stage == Stage.Second)
        {
            StartCoroutine(RunDialogueCoroutine(secondDialogue));
        }
        OnCutsceneFinished?.Invoke();
    }

    private IEnumerator RunDialogueCoroutine(List<DialogueLine> lines)
    {
        //turn off player


        textController.FadeInText();
        //Initial dialogue 
        rotatingCamera.Priority = 300;

        for (int i = 0; i < firstDialogue.Count; i++)
        {
            MakeCharacterTalk(firstDialogue[i]);

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
        rotatingCamera.Priority = 0;

        yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        textController.FadeOutText();
        //next scene
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
