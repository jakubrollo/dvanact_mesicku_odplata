using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public ScreenFader screenFader;
    public void PlayGame()
    {
        ScreenFader.Instance.FadeAndLoadScene("StoryboardIntro");
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
