using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PathReflector : MonoBehaviour
{
    [Header("Glare Settings")]
    [SerializeField] private Color glimmerColor = Color.cyan;
    [SerializeField] private float maxAlpha = 0.8f;
    // Lowered default focus: A wider angle allows the fade to start earlier and softer
    [SerializeField] private float glareFocus = 8.0f;
    [SerializeField] private float shimmerSpeed = 3.0f;

    [Header("Fade Settings")]
    [Tooltip("How fast it gets bright when you look at it. Try 2.0 with Lerp")]
    [SerializeField] private float fadeInSpeed = 2.0f;
    [Tooltip("How fast it disappears when you look away")]
    [SerializeField] private float fadeOutSpeed = 5.0f;

    [Header("Light Settings")]
    [Tooltip("Optional: Drag a child Point Light here to make it pulse with the sprite")]
    [SerializeField] private Light pointLight;
    [SerializeField] private float maxLightIntensity = 3.0f;

    [Header("References")]
    [SerializeField] private CandleBehavior playerCandle;

    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 0f;
    private Transform lightSourceTransform;

    void Start()
    // ... (Start method remains the same) ...
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerCandle == null)
            playerCandle = Object.FindFirstObjectByType<CandleBehavior>();

        if (playerCandle != null)
            lightSourceTransform = playerCandle.transform;

        Color c = glimmerColor;
        c.a = 0f;
        spriteRenderer.color = c;
        if (pointLight) pointLight.intensity = 0f;
    }

    void Update()
    {
        if (playerCandle == null || lightSourceTransform == null) return;

        // --- 1. Calculate Target ---
        float targetAlpha = 0f;

        if (playerCandle.candle_turned_on)
        {
            Vector3 directionToReflector = (transform.position - lightSourceTransform.position).normalized;
            float dot = Vector3.Dot(lightSourceTransform.forward, directionToReflector);

            if (dot > 0f)
            {
                // Note: If you lower glareFocus (e.g., from 20 to 8), 
                // the fade starts sooner as you turn, making it feel less sudden.
                float glareIntensity = Mathf.Pow(dot, glareFocus);

                // Clamp noise so it never goes fully to 0, ensuring consistent glow
                float shimmer = Mathf.PerlinNoise(Time.time * shimmerSpeed, 0f) * 0.2f + 0.8f;

                targetAlpha = maxAlpha * glareIntensity * shimmer;
            }
        }

        // --- 2. Organic Smoothing (Lerp) ---
        // Lerp feels more natural than MoveTowards for light. 
        // We use different speeds for in vs out.
        float speed = (targetAlpha > currentAlpha) ? fadeInSpeed : fadeOutSpeed;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * speed);

        // --- 3. Apply to Sprite ---
        Color finalColor = glimmerColor;
        finalColor.a = currentAlpha;
        spriteRenderer.color = finalColor;
        spriteRenderer.enabled = currentAlpha > 0.01f;

        // --- 4. Apply to Light with "EASE-IN" Curve ---
        if (pointLight != null)
        {
            // THE FIX: Square the alpha for the light calculation.
            // Example:
            // Linear: 0.1 Alpha = 0.3 Intensity (Visibly on)
            // Squared: 0.1 Alpha = 0.01 * 0.3 = 0.003 Intensity (Basically invisible)
            // This creates a curve where the light stays dim at the start and ramps up at the end.

            float percent = currentAlpha / maxAlpha; // Normalize 0 to 1
            float easeInCurve = percent * percent; // x^2 curve

            pointLight.intensity = easeInCurve * maxLightIntensity;

            // Only turn off renderer if strictly 0 to prevent flickering at low levels
            pointLight.enabled = currentAlpha > 0.001f;
        }
    }
}