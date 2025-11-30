using Cinemachine;
using NUnit.Framework;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum Stage
{
    First,
    Second
}


public class IntoOutroCutsceneManager : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;
    [SerializeField] private InputActionReference skipButton;

    [SerializeField] private DialogueTextController textController;

    //   [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private CinemachineVirtualCamera rotatingCamera;

    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();
    [SerializeField] private List<DialogueLine> secondDialogue = new List<DialogueLine>();

    [SerializeField] private int scream_index = 4;

    public UnityEvent OnCutsceneStart;
    public UnityEvent OnScreamInvoke;
    public UnityEvent OnCutsceneFinished;

    [SerializeField] private Stage stage = Stage.First;

    public void Start()
    {
        ActivateLunarDudesCutscene(IntroOutroInfoHolder.stageScene);
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

        for (int i = 0; i < lines.Count; i++)
        {
            MakeCharacterTalk(lines[i]);
            if (i == scream_index && stage == Stage.Second)
            {
                OnScreamInvoke?.Invoke();
            }

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
    //    rotatingCamera.Priority = 0;

       // yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        textController.FadeOutText();
        //next scene

        if(IntroOutroInfoHolder.stageScene == Stage.First)
        {
            ScreenFader.Instance.FadeAndLoadScene("Cabin");
        }
        else if(IntroOutroInfoHolder.stageScene == Stage.Second)
        {
            ScreenFader.Instance.FadeAndLoadScene("MainMenu");
        }

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
