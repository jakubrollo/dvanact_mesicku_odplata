using UnityEngine;
using UnityEngine.InputSystem;

public class CabinSceneManager : MonoBehaviour
{
    [SerializeField] private InputActionReference skipButton;
    [SerializeField] private GameObject player;

    [SerializeField] private DialogueTextController textController;
    [SerializeField] private CabinStage stage  = CabinStage.First;

    [SerializeField] private FirstStageManager firstStageManager;


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
            firstStageManager.RunStageFirst(textController, skipButton,player);
        }
        else if(stage == CabinStage.Second)
        { 
            RunStageSecond(); 
        }
        else if( stage == CabinStage.Third)
        {
            RunStageThird();
        }
    }



    private void RunStageSecond()
    {

    }

    private void RunStageThird()
    {

    }
}
