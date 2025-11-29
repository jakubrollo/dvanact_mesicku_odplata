using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PathReflector : MonoBehaviour
{
    [Header("Glare Settings")]
    [SerializeField] private Color glimmerColor = Color.cyan;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float glareFocus = 20.0f;
    [SerializeField] private float shimmerSpeed = 3.0f;

    [Header("Fade Settings")]
    [Tooltip("How fast it gets bright when you look at it (Lower = Slower buildup)")]
    [SerializeField] private float fadeInSpeed = 0.5f;
    [Tooltip("How fast it disappears when you look away (Higher = Snappy cleanup)")]
    [SerializeField] private float fadeOutSpeed = 4.0f;

    [Header("Light Settings")]
    [Tooltip("Optional: Drag a child Point Light here to make it pulse with the sprite")]
    [SerializeField] private Light pointLight;
    [SerializeField] private float maxLightIntensity = 1.0f;

    [Header("References")]
    [SerializeField] private PlayerCandle playerCandle;

    private SpriteRenderer spriteRenderer;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private Transform lightSourceTransform;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerCandle == null)
            playerCandle = Object.FindFirstObjectByType<PlayerCandle>();

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

        // 1. Calculate the DESIRED Alpha based on physics/angles
        float calculatedTarget = 0f;

        if (playerCandle.IsCandleOn)
        {
            Vector3 directionToReflector = (transform.position - lightSourceTransform.position).normalized;
            float dot = Vector3.Dot(lightSourceTransform.forward, directionToReflector);

            // Only reflect if looking roughly towards it
            if (dot > 0f)
            {
                float glareIntensity = Mathf.Pow(dot, glareFocus);
                float shimmer = Mathf.PerlinNoise(Time.time * shimmerSpeed, 0f) * 0.2f + 0.8f;
                calculatedTarget = maxAlpha * glareIntensity * shimmer;
            }
        }

        // 2. Determine which speed to use
        // If we want to get brighter (target > current), use the slow 'fadeInSpeed'.
        // If we want to hide (target < current), use the fast 'fadeOutSpeed'.
        float activeSpeed = (calculatedTarget > currentAlpha) ? fadeInSpeed : fadeOutSpeed;

        // 3. Apply Smooth Movement
        currentAlpha = Mathf.MoveTowards(currentAlpha, calculatedTarget, Time.deltaTime * activeSpeed);

        // 4. Apply to Sprite
        Color finalColor = glimmerColor;
        finalColor.a = currentAlpha;
        spriteRenderer.color = finalColor;

        // Optimization: Disable renderer if fully invisible
        spriteRenderer.enabled = currentAlpha > 0.01f;

        // 5. Apply to Light
        if (pointLight != null)
        {
            // The light intensity now follows the slow buildup too!
            pointLight.intensity = currentAlpha * maxLightIntensity;
            pointLight.enabled = currentAlpha > 0.01f;
        }
    }
}