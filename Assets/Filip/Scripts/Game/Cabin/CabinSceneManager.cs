using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CabinStage
{
    First,
    Second,
    Third,
    Fourth
}

public class CabinSceneManager : MonoBehaviour
{
    [SerializeField] private InputActionReference skipButton;
    [SerializeField] private GameObject player;

    [SerializeField] private DialogueTextController textController;

    [SerializeField] private FirstStageManager firstStageManager;
    [SerializeField] private SecondStageManager secondStageManager;
    [SerializeField] private ThirdStageManager thirdStageManager;
    [SerializeField] private FourthStageManager fourthStageManager;

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
    void Start()
    {
        RunCabinScene(GameProgressionManager.Instance.GetCurrentLevelData().CabinStage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunCabinScene(CabinStage stage)
    {
        if (stage == CabinStage.First)
        {

            firstStageManager.RunStage(textController, skipButton,player, characterMother, characterDaugther, dialogueCamera);
        }
        else if(stage == CabinStage.Second)
        {
            secondStageManager.RunStage(textController, skipButton, player, characterMother, characterDaugther, dialogueCamera);
        }
        else if( stage == CabinStage.Third)
        {
            thirdStageManager.RunStage(textController, skipButton, player, characterMother, characterDaugther, dialogueCamera);
        }
        else if (stage == CabinStage.Fourth)
        {
            fourthStageManager.RunStage(textController, skipButton, player, characterMother, characterDaugther, dialogueCamera);
        }
    }


}
