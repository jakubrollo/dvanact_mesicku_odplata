using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public ScreenFader screenFader;
    public void PlayGame()
    {
        ScreenFader.instance.FadeToScene("Game");
    }

    public void StartTutorial()
    {
        ScreenFader.instance.FadeToScene("Tutorial");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
