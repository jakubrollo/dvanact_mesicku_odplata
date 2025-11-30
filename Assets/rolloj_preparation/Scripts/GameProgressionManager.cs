using System.Linq;
using UnityEngine;
// We access Unity's SceneManager explicitly to avoid naming conflicts

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance;

    [System.Serializable]
    public struct LevelData
    {
        [Header("Scene Selection")]
        public string sceneName; // "Forest" or "Cabin"

        [Header("Scene Configuration")]
        [Tooltip("Which spawn point to use from the list in the scene")]
        public int spawnPointIndex;

        [Tooltip("Which exit/trigger/cutscene to activate in the scene")]
        public int activeEventIndex;

        public LunarDudesStage ForestStage;

        [Header("Enemy Configuration")]
        public bool hasEnemy;
        public int enemySpawnPointIndex; // Which spawn point for the enemy
        public float chaseSpeed;
        public float noticeBuildUpTime;
        public float maxDetectionRadius;
    }

    [Header("The Timeline")]
    [Tooltip("Define the order of the game here.")]
    [SerializeField] private LevelData[] progressionSteps;

    private int currentStepIndex = 0;

    //public AmbientClipsManager Ambients;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string CurrentSceneName()
    {
        if (progressionSteps.Count() >= currentStepIndex)
        {
            //print("progressionSteps index out of range!");
            return "Forest";
        }
        //print("Current scene is '" + progressionSteps[currentStepIndex].sceneName + "'!");
        return progressionSteps[currentStepIndex].sceneName;
    }

    public void StartGame()
    {
        currentStepIndex = 0;
        LoadCurrentStep();
    }
    public void ReloadCurrentLevel()
    {
        //Ambients.CloseDoorSound();

        // 1. Get current level data so we know the scene name
        LevelData step = progressionSteps[currentStepIndex];

        // 2. CHECK FOR FADER
        if (ScreenFader.Instance != null)
        {
            print("Using ScreenFader to reload current level: " + step.sceneName);
            // This triggers the visual Fade Out -> Load
            ScreenFader.Instance.FadeAndLoadScene(step.sceneName);
        }
        else
        {
            print("No ScreenFader found. Reloading current level directly: " + step.sceneName); 
            // Fallback if Fader is missing
            UnityEngine.SceneManagement.SceneManager.LoadScene(step.sceneName);
        }

        //print("Entering Ambients runner");
        //Ambients.RunAmbientMusicBasedOnScene(progressionSteps[currentStepIndex].sceneName);
    }

    public void LoadNextLevel()
    {
        currentStepIndex++;

        if (currentStepIndex >= progressionSteps.Length)
        {
            Debug.Log("Game Finished!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            Destroy(gameObject);
            return;
        }

        LoadCurrentStep();
    }

    private void LoadCurrentStep()
    {
        //Ambients.CloseDoorSound();

        LevelData step = progressionSteps[currentStepIndex];
        if (ScreenFader.Instance != null)
        {
            print("Using ScreenFader to reload current level: " + step.sceneName);
            // This triggers the visual Fade Out -> Load
            ScreenFader.Instance.FadeAndLoadScene(step.sceneName);
            // run ambient loop sounds
            //Ambients.RunAmbientMusicBasedOnScene(step.sceneName);
        }
        else
        {
            print("No ScreenFader found. Reloading current level directly: " + step.sceneName);
            // Fallback if Fader is missing
            UnityEngine.SceneManagement.SceneManager.LoadScene(step.sceneName);

	        // run ambient loop sounds
       	    //Ambients.RunAmbientMusicBasedOnScene(step.sceneName);
        }
    }

    public LevelData GetCurrentLevelData()
    {
        if (progressionSteps != null && currentStepIndex < progressionSteps.Length)
        {
            return progressionSteps[currentStepIndex];
        }
        return new LevelData();
    }
}