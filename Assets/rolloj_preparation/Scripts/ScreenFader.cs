using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Photon.Pun;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    [SerializeField] private Image img;
    [SerializeField] private float speed = 2f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // Force alpha to 1 (Black) instantly so we don't see a flash of the menu
        if (img != null)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
            img.raycastTarget = true;
        }

        StartCoroutine(FadeIn());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    // --- THE MISSING METHOD (The Bridge) ---
    public void FadeAndLoadScene(string sceneName)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // If we are online, only the Host can switch scenes
            if (PhotonNetwork.IsMasterClient)
            {
                FadeOutToNetworkScene(sceneName);
            }
        }
        else
        {
            // Offline / Singleplayer fallback
            StartCoroutine(FadeOutAndLoad(sceneName));
        }
    }

    public void FadeOutToNetworkScene(string sceneName)
    {
        StartCoroutine(FadeOutAndNetworkLoad(sceneName));
    }

    // --- COROUTINES ---

    public IEnumerator FadeOutLocal()
    {
        img.raycastTarget = true;
        Color c = img.color; c.a = 0f; img.color = c;
        while (c.a < 1f) { c.a += Time.deltaTime * speed; img.color = c; yield return null; }
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator FadeOutAndNetworkLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOutLocal()); // Reuse the local fade logic

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOutLocal());
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeIn()
    {
        img.raycastTarget = true;
        Color c = img.color; c.a = 1f; img.color = c;
        yield return new WaitForSeconds(0.5f);
        while (c.a > 0f) { c.a -= Time.deltaTime * speed; img.color = c; yield return null; }
        img.raycastTarget = false;
    }
}