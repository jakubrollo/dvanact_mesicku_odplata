using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FlashlightFollow : MonoBehaviour
{
    RectTransform rt;
    Material mat;
    Image img;

    [Header("Flashlight Settings")]
    [Range(0.0f, 1.0f)]
    public float baseRadius = 0.2f;

    [Header("Flicker Timing")]
    public bool enableFlicker = true;

    [Tooltip("Minimum time (seconds) to wait between flicker bursts.")]
    public float minWaitTime = 1.0f;

    [Tooltip("Maximum time (seconds) to wait between flicker bursts.")]
    public float maxWaitTime = 3.0f;

    [Header("Burst Settings")]
    [Tooltip("How long the flickering lasts when it happens.")]
    public float burstDuration = 0.4f;

    [Tooltip("How much the light dims during a flicker (0.1 is subtle, 0.5 is strong).")]
    public float flickerStrength = 0.3f;

    // Internal State
    private float nextFlickerTime;
    private float flickEndTime;
    private bool isFlickering = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        mat = img.material;

        // Schedule the first flicker randomly
        ScheduleNextFlicker();
    }

    void Update()
    {
        // --- 1. Position Logic (Standard) ---
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePos, null, out localPos);

        Vector2 uv = new Vector2(
            (localPos.x / rt.rect.width) + 0.5f,
            (localPos.y / rt.rect.height) + 0.5f
        );

        mat.SetVector("_Position", new Vector4(uv.x, uv.y, 0, 0));
        float aspect = rt.rect.width / rt.rect.height;
        mat.SetFloat("_AspectRatio", aspect);


        // --- 2. Intermittent Flicker Logic ---
        float currentRadius = baseRadius;

        if (enableFlicker)
        {
            // Check if it's time to START flickering
            if (!isFlickering && Time.time >= nextFlickerTime)
            {
                isFlickering = true;
                flickEndTime = Time.time + burstDuration;
            }

            // If we ARE flickering...
            if (isFlickering)
            {
                if (Time.time < flickEndTime)
                {
                    // While flickering, randomly shrink the radius
                    // Using Random.value gives a jagged, electric feel
                    float reduction = Random.value * flickerStrength;
                    currentRadius -= reduction;
                }
                else
                {
                    // Time is up, stop flickering and schedule the next one
                    isFlickering = false;
                    ScheduleNextFlicker();
                }
            }
        }

        // Apply final radius
        mat.SetFloat("_Radius", currentRadius);
    }

    void ScheduleNextFlicker()
    {
        // Pick a random time in the future
        nextFlickerTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
    }
}