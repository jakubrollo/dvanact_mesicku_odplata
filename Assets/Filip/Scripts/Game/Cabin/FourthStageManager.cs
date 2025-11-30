using Cinemachine;
using NUnit.Framework;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class FourthStageManager : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;

    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();

    [SerializeField] private Transform firstCameraSpot;

    private DialogueTextController dialogueTextController;

    private InputActionReference skipButton;
    private FirstPersonController firstPersonController;

    private GameObject characterMother;
    private GameObject characterDaugther;

    // private CinemachineVirtualCamera virtualCamera;
    private CinemachineVirtualCamera dialogueVirtualCamera;

    private GameObject player;
    [SerializeField] private Transform firstPCPos;
    [SerializeField] private Transform firstMotherPos;
    [SerializeField] private Transform firstDaughterPos;
    public UnityEvent OnStageFinished;
    public void RunStage(DialogueTextController dialogueTextController, InputActionReference skipButton, GameObject player, GameObject mother, GameObject daughter/*, CinemachineVirtualCamera camera*/, CinemachineVirtualCamera dialogueCamera)
    {
        this.dialogueTextController = dialogueTextController;
        this.skipButton = skipButton;
        this.firstPersonController = player.GetComponent<FirstPersonController>();
        //  virtualCamera = camera;
        this.player = player;
        this.characterMother = mother;
        this.characterDaugther = daughter;
        this.dialogueVirtualCamera = dialogueCamera;
        this.skipButton.action.Enable();
        daughter.GetComponent<Billboard>().StartAnimation();
        mother.GetComponent<Billboard>().StartAnimation();

        StartCoroutine(RunDialogueCoroutine());
    }



    private IEnumerator RunDialogueCoroutine()
    {
        player.transform.position = firstPCPos.position;
        characterMother.transform.position = firstMotherPos.position;
        characterDaugther.transform.position = firstDaughterPos.position;

        firstPersonController.canMove = false;

        dialogueTextController.FadeInText();
        //Initial dialogue 
        dialogueVirtualCamera.transform.position = firstCameraSpot.position;
        dialogueVirtualCamera.Priority = 300;

        dialogueVirtualCamera.LookAt = characterMother.transform;

        for (int i = 0; i < firstDialogue.Count; i++)
        {
            MakeCharacterTalk(firstDialogue[i]);

            if (firstDialogue[i].speakerName == "Macecha")
            {
                dialogueVirtualCamera.LookAt = characterMother.transform;
            }
            else if (firstDialogue[i].speakerName == "Holena")
            {
                dialogueVirtualCamera.LookAt = characterDaugther.transform;
            }

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
    //    dialogueVirtualCamera.Priority = 0;
     //   firstPersonController.canMove = true;

     //   yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        dialogueTextController.FadeOutText();
        //next scene
        OnStageFinished?.Invoke();
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

    private IEnumerator WaitForTime(float time)
    {
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;

            yield return null;
        }
    }

    private void MakeCharacterTalk(DialogueLine line)
    {
        //  if (character == null) return; make sure it doesnt call animation on player character

        dialogueTextController.UpdateTextInBox(line.lineText, line.speakerName);

    }
}
