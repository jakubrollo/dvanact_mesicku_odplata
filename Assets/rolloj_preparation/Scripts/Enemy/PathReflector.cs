using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PathReflector : MonoBehaviour
{
    [Header("Glimmer Settings")]
    [SerializeField] private Color glimmerColor = Color.cyan; // The color of the marker
    [SerializeField] private float pulseSpeed = 2.0f; // How fast it pulses
    [SerializeField] private float fadeSpeed = 5.0f;  // How fast it appears/disappears
    [SerializeField] private float maxAlpha = 0.8f;   // Max brightness (0 to 1)

    [SerializeField]  private PlayerCandle playerCandle;
    private SpriteRenderer spriteRenderer;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();


        // Start invisible
        Color c = glimmerColor;
        c.a = 0f;
        spriteRenderer.color = c;
    }

    void Update()
    {
        if (playerCandle == null) return;

        // 1. Determine Target Alpha
        if (playerCandle.IsCandleOn)
        {
            // Calculate a pulsing sine wave (Glimmer effect)
            // Moves between 0.3 and 1.0 based on time
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            // Map it to a nice range so it never goes fully dark while ON
            float glimmerFactor = Mathf.Lerp(0.4f, 1.0f, pulse);

            targetAlpha = maxAlpha * glimmerFactor;
        }
        else
        {
            targetAlpha = 0f; // Invisible
        }

        // 2. Smoothly Lerp current alpha to target
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // 3. Apply Color
        Color finalColor = glimmerColor;
        finalColor.a = currentAlpha;
        spriteRenderer.color = finalColor;

        // Optimization: Disable renderer if fully invisible to save performance
        spriteRenderer.enabled = currentAlpha > 0.01f;
    }
}