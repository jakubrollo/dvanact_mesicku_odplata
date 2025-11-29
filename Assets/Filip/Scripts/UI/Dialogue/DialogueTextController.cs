using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueTextController : MonoBehaviour
{
    [SerializeField] private TMP_Text textMeshPro;
    [SerializeField] private TMP_Text textMeshProName;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textMeshTips;


    [SerializeField] private float charDelay = 0.05f;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float tipDisplayTime = 10f;
    [SerializeField] private float tipFadeTime = 7f;

    private bool dialogueVisible = false;


    private Coroutine tipCoroutine;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TMP_Text>();


    }

    [ContextMenu("Test Update Text")]
    private void TestUpdateText()
    {
        UpdateTextInBox("Hello from Inspector!", "rizzler");
    }

    public void UpdateTextInBox(string newText, string characterName)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        textMeshProName.text = characterName;

        typingCoroutine = StartCoroutine(TypeText(newText));
    }


    // Zavolá se na zaèátku dialogu
    public void FadeInText()
    {
        HideTip();
        dialogueVisible = true;
        dialogueBox.SetActive(true);
        StartCoroutine(FadeText(textMeshPro, 0f, 1f, fadeDuration));
    }

    // Zavolá se na konci dialogu
    public void FadeOutText()
    {
        StartCoroutine(FadeText(textMeshPro,1f, 0f, fadeDuration));
        dialogueBox.SetActive(false);
        dialogueVisible = false;
    }

    private IEnumerator FadeText(TMP_Text tmp, float startA, float endA, float duration)
    {
        float t = 0f;
        Color c = tmp.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startA, endA, t / duration);
            tmp.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        tmp.color = new Color(c.r, c.g, c.b, endA);
    }

    private IEnumerator TypeText(string newText)
    {
        textMeshPro.text = ""; 

        foreach (char c in newText)
        {
            textMeshPro.text += c; 
            yield return new WaitForSeconds(charDelay);
        }

        typingCoroutine = null; 
    }


    public void ShowTip(string tip)
    {
        if (tipCoroutine != null)
            StopCoroutine(tipCoroutine);

        tipCoroutine = StartCoroutine(ShowTipCoroutine(tip));
    }

    public void HideTip(float fastFade = 0.1f)
    {
        // Stop current tip coroutine (showing / fading in / waiting)
        if (tipCoroutine != null)
        {
            StopCoroutine(tipCoroutine);
            tipCoroutine = null;
        }

        // If still active, fade out fast
        if (textMeshTips.gameObject.activeSelf)
        {
            // Start a quick fade-out coroutine
            StartCoroutine(FastHideTipCoroutine(fastFade));
        }
    }

    private IEnumerator FastHideTipCoroutine(float fadeTime)
    {
        // Fade from current alpha to 0
        yield return FadeText(textMeshTips, textMeshTips.alpha, 0f, fadeTime);

        textMeshTips.gameObject.SetActive(false);
    }

    private IEnumerator ShowTipCoroutine(string tip)
    {
        while (dialogueVisible)
            yield return null;

        textMeshTips.gameObject.SetActive(true);
        textMeshTips.text = tip;

        yield return FadeText(textMeshTips, 0f, 1f, tipFadeTime);

        float timer = 0;
        while (timer < tipDisplayTime)
        {
            if (dialogueVisible)
                break;

            timer += Time.deltaTime;
            yield return null;
        }
        yield return FadeText(textMeshTips, 1f, 0f, tipFadeTime);
        textMeshTips.gameObject.SetActive(false);
    }
}
