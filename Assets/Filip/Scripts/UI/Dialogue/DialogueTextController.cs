using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueTextController : MonoBehaviour
{
    [SerializeField] private TMP_Text textMeshPro;
    [SerializeField] private TMP_Text textMeshProName;
    [SerializeField] private float charDelay = 0.05f;

    [SerializeField] private float fadeDuration = 1f;

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
        gameObject.SetActive(true);
        StartCoroutine(FadeText(0f, 1f, fadeDuration));
    }

    // Zavolá se na konci dialogu
    public void FadeOutText()
    {
        StartCoroutine(FadeText(1f, 0f, fadeDuration));
        gameObject.SetActive(false);
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color color = textMeshPro.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            textMeshPro.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        textMeshPro.color = new Color(color.r, color.g, color.b, endAlpha);
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
}
