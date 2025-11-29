using Cinemachine;
using NUnit.Framework;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LunarDudesController : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;
    [SerializeField] private InputActionReference skipButton;

    [SerializeField] private DialogueTextController textController;

    //   [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private CinemachineVirtualCamera rotatingCamera;

    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();
    [SerializeField] private List<DialogueLine> secondDialogue = new List<DialogueLine>();
    [SerializeField] private List<DialogueLine> thirdDialogue = new List<DialogueLine>();


    public UnityEvent OnLunarCutsceneStart;
    public UnityEvent OnLunarCutsceneFinished;

    public enum LunarDudesStage
    {
        First,
        Second,
        Third
    }

    [SerializeField] LunarDudesStage stage  = LunarDudesStage.First;

    public void Start()
    {
        ActivateLunarDudesCutscene(stage);
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

        OnLunarCutsceneFinished?.Invoke();
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
