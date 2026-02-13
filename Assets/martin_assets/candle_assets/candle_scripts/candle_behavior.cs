using UnityEngine;
using UnityEngine.InputSystem;

public class CandleBehavior : MonoBehaviour
{
    // Configuration Fields
    public bool candle_turned_on = false;
    public float amplitude_movement_turned_on = 0.05f; // Adjusted for local space (0.05 meters is more realistic)
    public float floating_frequency_turned_on = 2f;

    [Tooltip("The position in LOCAL SPACE when the candle is off (default is initial placement).")]
    public Vector3 off_local_position;

    public float tween_duration = 0.5f; // How long the tween takes

    [Tooltip("The LOCAL SPACE offset added to off_local_position when the candle is turned on.")]
    public Vector3 on_local_position_offset = new Vector3(0, 0.1f, 0); // Changed to local offset for clarity

    public GameObject flame_plane; // The flame plane
    public float flame_scale_duration = 0.5f; // How long the flame scale animation takes
    public Light[] flame_lights; // The spotlights simulating the flame

    public CandleAudio audio_script;

    // Material Properties (still using runtimeMaterialInstance logic from previous turn)
    public float emission_intensity_on = 1f; // Emission intensity when on
    public float emission_intensity_off = 0f; // Emission intensity when off

    // Private State Fields
    private InputSystem_Actions inputActions;
    private float time_counter = 0f;

    // Position tracking in LOCAL SPACE
    private Vector3 starting_local_position; // Baseline for oscillation when ON
    private Vector3 tween_start_local_position;

    // For material instance access:
    private Material runtimeMaterialInstance;

    // For tweening
    private float tween_time = 0f;
    private bool is_tweening_turning_on = false;
    private bool is_tweening_turning_off = false;

    // For flame scaling
    private float flame_scale_time = 0f;
    private bool is_flame_scaling_up = false;
    private bool is_flame_scaling_down = false;
    private Vector3 original_flame_scale;
    private float[] original_light_intensities;
    private Color base_emission_color;

    private void Awake()
    {
        audio_script = GetComponent<CandleAudio>();
        inputActions = new InputSystem_Actions();

        // --- 1. Position Setup (Use LOCAL SPACE) ---
        // Capture the candle's initial position RELATIVE TO THE PARENT (the camera/player).
        off_local_position = transform.localPosition;
        starting_local_position = off_local_position; // Initialize baseline

        // --- 2. Material Setup (Runtime Instance) ---
        Renderer candleRenderer = GetComponent<Renderer>();
        if (candleRenderer != null)
        {
            // Accessing .material creates a unique runtime copy.
            runtimeMaterialInstance = candleRenderer.material;
        }
        else
        {
         //   Debug.LogError("CandleBehavior requires a Renderer component on the same GameObject to access the material.");
        }

        // --- 3. Light & Flame Setup ---
        if (flame_plane != null)
        {
            original_flame_scale = flame_plane.transform.localScale;
            if (!candle_turned_on)
            {
                flame_plane.gameObject.SetActive(false);
            }
        }

        if (flame_lights != null && flame_lights.Length > 0)
        {
            original_light_intensities = new float[flame_lights.Length];
            for (int i = 0; i < flame_lights.Length; i++)
            {
                if (flame_lights[i] != null)
                {
                    original_light_intensities[i] = flame_lights[i].intensity;
                    if (!candle_turned_on)
                    {
                        flame_lights[i].intensity = 0f;
                    }
                }
            }
        }

        if (runtimeMaterialInstance != null)
        {
            base_emission_color = runtimeMaterialInstance.GetColor("_EmissionColor");
            float maxComponent = Mathf.Max(base_emission_color.r, base_emission_color.g, base_emission_color.b);
            if (maxComponent > 0)
            {
                base_emission_color /= maxComponent;
            }

            // Set initial emission state using the instance
            if (candle_turned_on)
            {
                runtimeMaterialInstance.SetColor("_EmissionColor", base_emission_color * emission_intensity_on);
            }
            else
            {
                runtimeMaterialInstance.SetColor("_EmissionColor", base_emission_color * emission_intensity_off);
            }
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Player.CandleToggle.performed += OnToggleCandle;
            inputActions.Enable();
        }
    }


    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.CandleToggle.performed -= OnToggleCandle;
            inputActions.Disable();
        }
    }

    private void OnToggleCandle(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        Debug.Log("CandleToggled!");
        ToggleCandle();
    }

    public void ToggleCandle()
    {
        if (is_tweening_turning_off || is_tweening_turning_on) return;

        candle_turned_on = !candle_turned_on;
        if (candle_turned_on)
        {
            audio_script.TurnOn();
            ActivateLight();
        }
        else
        {
            audio_script.TurnOff();
            DeactivateLight();
        }
    }

    private void Update()
    {
        HandleCandlePosition();
        HandleFlameScale();
    }

    private void ActivateLight()
    {
        tween_start_local_position = transform.localPosition; // Use local position
        tween_time = 0f;
        is_tweening_turning_on = true;
        time_counter = 0f;

        // Start flame scaling up
        if (flame_plane != null)
        {
            flame_plane.gameObject.SetActive(true);
            flame_plane.transform.localScale = new Vector3(0, original_flame_scale.y, original_flame_scale.z);

            flame_scale_time = 0f;
            is_flame_scaling_up = true;
            is_flame_scaling_down = false;
        }
    }

    private void DeactivateLight()
    {
        tween_start_local_position = transform.localPosition; // Use local position
        tween_time = 0f;
        is_tweening_turning_off = true;

        // Start flame scaling down
        if (flame_plane != null)
        {
            flame_scale_time = 0f;
            is_flame_scaling_down = true;
            is_flame_scaling_up = false;
        }
    }

    private void HandleFlameScale()
    {
        // ... (Flame scale logic remains the same, only uses runtimeMaterialInstance)
        if (flame_plane == null) return;

        if (is_flame_scaling_up)
        {
            flame_scale_time += Time.deltaTime;
            float t = Mathf.Clamp01(flame_scale_time / flame_scale_duration);
            t = t * t * (3f - 2f * t);

            float current_x_scale = Mathf.Lerp(0, original_flame_scale.x, t);
            flame_plane.transform.localScale = new Vector3(current_x_scale, original_flame_scale.y, original_flame_scale.z);

            if (flame_lights != null)
            {
                for (int i = 0; i < flame_lights.Length; i++)
                {
                    if (flame_lights[i] != null)
                    {
                        flame_lights[i].intensity = Mathf.Lerp(0, original_light_intensities[i], t);
                    }
                }
            }

            if (runtimeMaterialInstance != null)
            {
                float current_emission = Mathf.Lerp(emission_intensity_off, emission_intensity_on, t);
                runtimeMaterialInstance.SetColor("_EmissionColor", base_emission_color * current_emission);
            }

            if (t >= 1f)
            {
                is_flame_scaling_up = false;
            }
        }
        else if (is_flame_scaling_down)
        {
            flame_scale_time += Time.deltaTime;
            float t = Mathf.Clamp01(flame_scale_time / flame_scale_duration);
            t = t * t * (3f - 2f * t);

            float current_x_scale = Mathf.Lerp(original_flame_scale.x, 0, t);
            flame_plane.transform.localScale = new Vector3(current_x_scale, original_flame_scale.y, original_flame_scale.z);

            if (flame_lights != null)
            {
                for (int i = 0; i < flame_lights.Length; i++)
                {
                    if (flame_lights[i] != null)
                    {
                        flame_lights[i].intensity = Mathf.Lerp(original_light_intensities[i], 0, t);
                    }
                }
            }

            if (runtimeMaterialInstance != null)
            {
                float current_emission = Mathf.Lerp(emission_intensity_on, emission_intensity_off, t);
                runtimeMaterialInstance.SetColor("_EmissionColor", base_emission_color * current_emission);
            }

            if (t >= 1f)
            {
                is_flame_scaling_down = false;
                flame_plane.gameObject.SetActive(false);
            }
        }
    }

    private void HandleCandlePosition()
    {
        // Target position when ON, relative to the parent
        Vector3 on_target_local_position = off_local_position + on_local_position_offset;

        if (is_tweening_turning_on)
        {
            // Tween from the current local position to the 'on' target local position
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);
            t = t * t * (3f - 2f * t);

            transform.localPosition = Vector3.Lerp(tween_start_local_position, on_target_local_position, t);

            if (t >= 1f)
            {
                is_tweening_turning_on = false;
                // Once the tween finishes, this becomes the baseline for the sine wave oscillation
                starting_local_position = transform.localPosition;
            }
        }
        else if (candle_turned_on) // Floating oscillation when ON is stable
        {
            time_counter += Time.deltaTime * floating_frequency_turned_on;
            float y_offset = Mathf.Sin(time_counter) * amplitude_movement_turned_on;

            // Apply oscillation based on the stable starting_local_position
            transform.localPosition = starting_local_position + new Vector3(0, y_offset, 0);
        }
        else if (is_tweening_turning_off)
        {
            // Tween from the current local position (which might have been oscillating) back to the OFF position
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);
            t = t * t * (3f - 2f * t);

            transform.localPosition = Vector3.Lerp(tween_start_local_position, off_local_position, t);

            if (t >= 1f)
            {
                is_tweening_turning_off = false;
                // Lock position exactly to off_local_position when finished
                transform.localPosition = off_local_position;
            }
        }
        else // Candle is OFF and not tweening
        {
            // Ensures the candle is exactly at the off position and not drifting
            transform.localPosition = off_local_position;
        }
    }
}