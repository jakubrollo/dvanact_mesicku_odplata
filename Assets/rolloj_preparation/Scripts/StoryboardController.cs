using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class StoryboardController : MonoBehaviour
{
    public Sprite[] slides;
    public Image image;
    public Image fadeOverlay;
    public TextMeshProUGUI continueText;

    [Header("Settings")]
    public float fadeTime = 1.5f;     // Time to fade from Black -> Image
    public float fadeOutTime = 0.5f;  // Time to fade from Image -> Black (NEW)
    public float continueDelay = 1.5f;
    public float continueFadeTime = 1f;

    // State Variables
    int index = -1;
    float t;

    // We split "fading" into two distinct states
    bool isFadingIn;
    bool isFadingOut;
    bool fullyShown;

    // Text Variables
    float continueT;
    bool continueFading;

    void Start()
    {
        NextSlide();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // 1. If Fading IN: Skip to fully shown
            if (isFadingIn)
            {
                t = fadeTime;
                FadeInUpdate();

                fullyShown = true;
                isFadingIn = false;
                fadeOverlay.color = new Color(0, 0, 0, 0);
                StartTextTimer();
            }
            // 2. If Fully Shown: Start Fading OUT
            else if (fullyShown)
            {
                StartFadeOut();
            }
            // 3. If Fading OUT: Skip to Next Slide immediately
            else if (isFadingOut)
            {
                NextSlide();
            }
        }

        if (isFadingIn) FadeInUpdate();
        if (isFadingOut) FadeOutUpdate();
        if (continueFading) ContinueFadeUpdate();
    }

    void NextSlide()
    {
        index++;
        if (index >= slides.Length)
        {
            SceneManager.LoadScene("Cabin");
            return;
        }

        image.sprite = slides[index];

        // Reset Logic
        t = 0;
        isFadingIn = true;
        isFadingOut = false;
        fullyShown = false;

        // Ensure screen is black before we start fading in
        fadeOverlay.color = new Color(0, 0, 0, 1);

        // Reset Text
        continueText.color = new Color(1, 1, 1, 0);
        continueFading = false;
        continueT = 0;
    }

    void StartFadeOut()
    {
        fullyShown = false;
        isFadingOut = true;
        t = 0;

        // Hide text immediately
        continueFading = false;
        continueText.color = new Color(1, 1, 1, 0);
    }

    void FadeInUpdate()
    {
        t += Time.deltaTime;
        // Uses the slow 'fadeTime'
        float a = 1 - Mathf.Clamp01(t / fadeTime);
        fadeOverlay.color = new Color(0, 0, 0, a);

        if (a <= 0)
        {
            isFadingIn = false;
            fullyShown = true;
            StartTextTimer();
        }
    }

    void FadeOutUpdate()
    {
        t += Time.deltaTime;

        // --- CHANGED HERE ---
        // Uses the faster 'fadeOutTime'
        float a = Mathf.Clamp01(t / fadeOutTime);
        fadeOverlay.color = new Color(0, 0, 0, a);

        // Once fully black (alpha >= 1), load the next slide
        if (a >= 1)
        {
            NextSlide();
        }
    }

    void StartTextTimer()
    {
        if (!continueFading)
        {
            continueT = 0;
            continueFading = true;
            continueText.color = new Color(1, 1, 1, 0);
        }
    }

    void ContinueFadeUpdate()
    {
        if (continueT < continueDelay)
        {
            continueT += Time.deltaTime;
            return;
        }

        float timeSinceDelay = continueT - continueDelay;
        float a = Mathf.Clamp01(timeSinceDelay / continueFadeTime);
        continueText.color = new Color(1, 1, 1, a);
        continueT += Time.deltaTime;
    }
}