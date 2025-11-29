using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PathReflector : MonoBehaviour
{
    [Header("Glare Settings")]
    [SerializeField] private Color glimmerColor = Color.cyan;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float glareFocus = 20.0f;
    [SerializeField] private float shimmerSpeed = 3.0f;

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

        Debug.Log("PathReflector: Found Point Light: " + (pointLight != null));
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

        if (playerCandle.IsCandleOn)
        {
            Vector3 directionToReflector = (transform.position - lightSourceTransform.position).normalized;
            float dot = Vector3.Dot(lightSourceTransform.forward, directionToReflector);

            if (dot > 0f)
            {
                float glareIntensity = Mathf.Pow(dot, glareFocus);
                float shimmer = Mathf.PerlinNoise(Time.time * shimmerSpeed, 0f) * 0.2f + 0.8f;
                targetAlpha = maxAlpha * glareIntensity * shimmer;
            }
            else
            {
                targetAlpha = 0f;
                if(pointLight != null)
                    pointLight.enabled = false;
            }
        }
        else
        {
            targetAlpha = 0f;
            if (pointLight != null)
                pointLight.enabled = false;
        }


        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * 10f);


        Color finalColor = glimmerColor;
        finalColor.a = currentAlpha;
        spriteRenderer.color = finalColor;
        spriteRenderer.enabled = currentAlpha > 0.01f;

        if (pointLight != null)
        {
            pointLight.intensity = currentAlpha * maxLightIntensity;

            pointLight.enabled = currentAlpha > 0.01f;
        }
    }
}