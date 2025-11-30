using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public ScreenFader screenFader;
    public void PlayGame()
    {
        IntroOutroInfoHolder.stageScene = Stage.First;
        ScreenFader.Instance.FadeAndLoadScene("ForestCutscene");
    }

    public void StartTutorial()
    {
        ScreenFader.Instance.FadeAndLoadScene("Tutorial");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
