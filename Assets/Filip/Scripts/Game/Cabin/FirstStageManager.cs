using NUnit.Framework;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class FirstStageManager : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;


    [SerializeField] private GameObject characterMother;
    [SerializeField] private GameObject characterDaugther;
    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();

    private DialogueTextController dialogueTextController;

    private InputActionReference skipButton;
    private FirstPersonController firstPersonController;

    [SerializeField] private List<DialogueLine> secondDialogue = new List<DialogueLine>();

    public bool PlayerFoundMirror = false;
    public void RunStageFirst(DialogueTextController dialogueTextController, InputActionReference skipButton, GameObject player)
    {
        this.dialogueTextController = dialogueTextController;
        this.skipButton = skipButton;
        this.firstPersonController = player.GetComponent<FirstPersonController>();
        StartCoroutine(RunDialogueCoroutine());
    }



    private IEnumerator RunDialogueCoroutine()
    {
        firstPersonController.canMove = false;

        dialogueTextController.FadeInText();
            //Initial dialogue 
        for (int i = 0; i < firstDialogue.Count; i++)
        {
            MakeCharacterTalk(firstDialogue[i]);
            
            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }

        firstPersonController.canMove = true;

        yield return StartCoroutine(WaitForPlayerFindMirror());


        //Second dialogue 
        for (int i = 0; i < secondDialogue.Count; i++)
        {
            MakeCharacterTalk(secondDialogue[i]);

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
        yield return StartCoroutine (WaitForTimeOrSkip(pauseBetweenLines));
        dialogueTextController.FadeOutText();
        //next scene
        Debug.Log("next scene");
    }

    private IEnumerator WaitForTimeOrSkip(float time)
    {
        float timer = 0f;

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

    private IEnumerator WaitForTime(float time)
    {
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator WaitForPlayerFindMirror()
    {

        PlayerFoundMirror = false;
        while (!PlayerFoundMirror)
        {
            yield return null; 
        }
    }

    private void MakeCharacterTalk(DialogueLine line)
    {
      //  if (character == null) return; make sure it doesnt call animation on player character

        dialogueTextController.UpdateTextInBox(line.lineText, line.speakerName);

    }
}
