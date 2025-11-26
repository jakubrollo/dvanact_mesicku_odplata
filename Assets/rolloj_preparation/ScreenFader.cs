using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;
    public Image img;
    public float speed = 1f;

    void Awake() { instance = this; }

    public void FadeToScene(string scene)
    {
        img.raycastTarget = true;
        StartCoroutine(Fade(scene));
    }

    System.Collections.IEnumerator Fade(string scene)
    {
        Color c = img.color;
        while (c.a < 1f)
        {
            c.a = Mathf.Lerp(c.a, 1f, Time.deltaTime * speed);
            img.color = c;
            yield return null;
        }
        SceneManager.LoadScene(scene);
    }
}
