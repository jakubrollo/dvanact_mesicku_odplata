using Cinemachine;
using NUnit.Framework;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;


public class FirstStageManager : MonoBehaviour
{
    [SerializeField] private float pauseBetweenLines = 3f;

    [SerializeField] private List<DialogueLine> firstDialogue = new List<DialogueLine>();

    [SerializeField] private Transform firstCameraSpot;
    [SerializeField] private Transform secondCameraSpot;

    [SerializeField] private string lanternTip = "Press left mouse button, to find reflecting objects.";

    private DialogueTextController dialogueTextController;

    private InputActionReference skipButton;
    private FirstPersonController firstPersonController;

    private GameObject characterMother;
    private GameObject characterDaugther;

 //   private CinemachineVirtualCamera virtualCamera;
    private CinemachineVirtualCamera dialogueVirtualCamera;

    [SerializeField] private List<DialogueLine> secondDialogue = new List<DialogueLine>();

    private GameObject player;
    [SerializeField] private Transform firstPCPos;
    [SerializeField] private Transform firstMotherPos;
    [SerializeField] private Transform firstDaughterPos;

    [SerializeField] private Transform secondMotherPos;
    //   [SerializeField] private Transform secondDaughterPos;

    public UnityEvent OnStageFinished;

    public bool PlayerFoundMirror = false;
    public void RunStage(DialogueTextController dialogueTextController, InputActionReference skipButton, GameObject player, GameObject mother, GameObject daughter, CinemachineVirtualCamera dialogueCamera)
    {
        this.dialogueTextController = dialogueTextController;
        this.skipButton = skipButton;
        this.firstPersonController = player.GetComponent<FirstPersonController>();
       // virtualCamera = camera;
        this.player = player;
        this.characterMother = mother;
        this.characterDaugther = daughter;
        this.dialogueVirtualCamera = dialogueCamera;
        this.skipButton.action.Enable();

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
        
        for (int i = 0; i < firstDialogue.Count; i++)
        {
            MakeCharacterTalk(firstDialogue[i]);

            if (firstDialogue[i].speakerName == "Macecha")
            {
                dialogueVirtualCamera.LookAt = characterMother.transform;
            }

            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
        dialogueVirtualCamera.Priority = 0;
        firstPersonController.canMove = true;


        dialogueTextController.FadeOutText();
        dialogueTextController.ShowTip(lanternTip);



        yield return StartCoroutine(WaitForPlayerFindMirror());
        dialogueTextController.FadeInText();

        dialogueVirtualCamera.transform.position = secondCameraSpot.position;
        dialogueVirtualCamera.Priority = 300;

//        characterMother.transform.position = secondMotherPos.position;
    //    characterDaugther.transform.position = secondDaughterPos.position;

        //Second dialogue 
        for (int i = 0; i < secondDialogue.Count; i++)
        {
            MakeCharacterTalk(secondDialogue[i]);

            if (secondDialogue[i].speakerName == "Macecha")
            {
                characterMother.transform.position = secondMotherPos.position;
                dialogueVirtualCamera.LookAt = characterMother.transform;
            }
            else if(secondDialogue[i].speakerName == "Holena")
            {
                dialogueVirtualCamera.LookAt = characterDaugther.transform;
            }
            yield return StartCoroutine(WaitForTimeOrSkip(pauseBetweenLines));
        }
        dialogueVirtualCamera.Priority = 0;

        yield return StartCoroutine (WaitForTimeOrSkip(pauseBetweenLines));
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
