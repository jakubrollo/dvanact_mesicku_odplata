using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    
    public void MainMenu()
    {
        ScreenFader.Instance.FadeAndLoadScene("MainMenu");
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            Debug.Log("Pointer over UI!");
    }

}
