using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    [SerializeField] private Image img;
    [SerializeField] private float speed = 2f;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            // Ensure this lives as long as the Manager does
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Always fade IN when the game starts or a scene loads
        StartCoroutine(FadeIn());
    }

    // Call this to load a scene with a fade
    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeIn()
    {
        Color c = img.color;
        c.a = 1f; // Start Black
        img.color = c;
        img.raycastTarget = false; // Let clicks through

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * speed;
            img.color = c;
            yield return null;
        }
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        img.raycastTarget = true; // Block clicks
        Color c = img.color;
        c.a = 0f; // Start Transparent
        img.color = c;

        while (c.a < 1f)
        {
            c.a += Time.deltaTime * speed;
            img.color = c;
            yield return null;
        }

        // Wait a tiny moment on black
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(sceneName);
        
        // Wait for scene to load, then fade in
        yield return null; 
        StartCoroutine(FadeIn());
    }
}