using Cinemachine;
using Photon.Pun;
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
    [Header("Multiplayer Setup")]
    [SerializeField] private string playerPrefabName = "Whole Player Object Variant Variant";
    [SerializeField] private Transform spawnPoint;

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
        if (PhotonNetwork.IsMasterClient)
        {
            // Mùžeš použít starý Input.GetKeyDown, pokud nemáš nadefinovanou akci v Input Systemu
            if (Input.GetKeyDown(KeyCode.G)) 
            {
                Debug.Log("Master Client maèká G -> Naèítám Forest");

                // DÙLEŽITÉ: Použít LoadLevel, ne SceneManager.LoadScene!
                // spis sem dat fader, ale pro test tohle bude staèit
                GameProgressionManager.Instance.LoadNextLevel();
            }
        }
    }

    public void RunCabinScene(CabinStage stage)
    {
        if (stage == CabinStage.First)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 spawnPosition = (spawnPoint != null) ? spawnPoint.position + randomOffset + (Vector3.up * 2f) : new Vector3(0, 2, 0);

            object[] myCustomData = new object[] { PhotonNetwork.NickName };

            // 1. Capture the new player object in the variable
            player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity, 0, myCustomData);

            // 2. FIND THE AMBIENT MANAGER
            // Based on your SceneInitializer code, the AmbientManager seems to be a child of the ProgressionManager
            AmbientClipsManager ambientManager = GameProgressionManager.Instance.GetComponentInChildren<AmbientClipsManager>();

            // If not found there, try finding it in the scene generally
            if (ambientManager == null)
            {
                ambientManager = FindObjectOfType<AmbientClipsManager>();
                if (ambientManager != null) ambientManager.WalkingAudioSource = player.GetComponent<AudioSource>();
            }

            // 3. ASSIGN THE AUDIO SOURCE
            if (ambientManager != null && player != null)
            {
                // Try to find an AudioSource on the player to use for walking
                AudioSource playerAudio = player.GetComponent<AudioSource>();

                // If the player doesn't have one, add one, or find a specific child object
                if (playerAudio == null) playerAudio = player.AddComponent<AudioSource>();

                // Assign it to the manager so footsteps play from the player's location
                ambientManager.WalkingAudioSource = playerAudio;

            }
        }
        //firstStageManager.RunStage(textController, skipButton,player, characterMother, characterDaugther, dialogueCamera);
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
