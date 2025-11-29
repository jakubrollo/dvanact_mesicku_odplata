using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CabinSceneManager : MonoBehaviour
{
    [SerializeField] private InputActionReference skipButton;
    [SerializeField] private GameObject player;

    [SerializeField] private DialogueTextController textController;
    [SerializeField] private CabinStage stage  = CabinStage.First;

    [SerializeField] private FirstStageManager firstStageManager;
    [SerializeField] private SecondStageManager secondStageManager;

 //   [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private CinemachineVirtualCamera dialogueCamera;

    [SerializeField] private GameObject characterMother;
    [SerializeField] private GameObject characterDaugther;

//    [SerializeField] private Transform firstPCPos;
//    [SerializeField] private Transform firstMotherPos;
//    [SerializeField] private Transform firstDaughterPos;

    [SerializeField] private Transform secondPCPos;
    [SerializeField] private Transform secondMotherPos;
    [SerializeField] private Transform secondDaughterPos;
    public enum CabinStage
    {
        First,
        Second,
        Third
    }
    void Start()
    {
        RunCabinScene(stage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunCabinScene(CabinStage stage)
    {
        if (stage == CabinStage.First)
        {

            firstStageManager.RunStageFirst(textController, skipButton,player, characterMother, characterDaugther, dialogueCamera);
        }
        else if(stage == CabinStage.Second)
        {
            secondStageManager.RunStageSecond(textController, skipButton, player, characterMother, characterDaugther, dialogueCamera);
        }
        else if( stage == CabinStage.Third)
        {
          //  RunStageThird();
        }
    }


}
