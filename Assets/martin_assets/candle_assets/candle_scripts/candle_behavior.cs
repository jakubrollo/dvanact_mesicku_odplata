using UnityEngine;
using UnityEngine.InputSystem;
public class CandleBehavior : MonoBehaviour
{
    public bool candle_turned_on = false;
    public float amplitude_movement_turned_on = 10f;
    public float floating_frequency_turned_on = 2f;
    public Vector3 off_position = Vector3.zero; // Position when candle is off
    public float tween_duration = 0.5f; // How long the tween takes
    public Vector3 on_position_offset = Vector3.zero; // Position when candle is off
    public GameObject flame_plane; // The flame plane
    public float flame_scale_duration = 0.5f; // How long the flame scale animation takes
    public Light[] flame_lights; // The spotlights simulating the flame
    public Material candle_material; // The candle body material
    public float emission_intensity_on = 1f; // Emission intensity when on
    public float emission_intensity_off = 0f; // Emission intensity when off

    private InputSystem_Actions inputActions;
    private Vector3 starting_position;
    private float time_counter = 0f;
    // For tweening

    private Vector3 tween_start_position;
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
        inputActions = new InputSystem_Actions();
        starting_position = transform.position;
        off_position = transform.position;

        // Store original flame scale and set initial state
        if (flame_plane != null)
        {
            original_flame_scale = flame_plane.transform.localScale;
            if (!candle_turned_on)
            {
                flame_plane.gameObject.SetActive(false);
            }
        }

        // Store original light intensities
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

        // Store base emission color and set initial emission state
        if (candle_material != null)
        {
            base_emission_color = candle_material.GetColor("_EmissionColor");
            // Normalize to get base color without intensity
            float maxComponent = Mathf.Max(base_emission_color.r, base_emission_color.g, base_emission_color.b);
            if (maxComponent > 0)
            {
                base_emission_color = base_emission_color / maxComponent;
            }

            if (candle_turned_on)
            {
                candle_material.SetColor("_EmissionColor", base_emission_color * emission_intensity_on);
            }
            else
            {
                candle_material.SetColor("_EmissionColor", base_emission_color * emission_intensity_off);
            }
        }
    }
    private void OnEnable()
    {
        inputActions.Player.CandleToggle.performed += OnToggleCandle;
        inputActions.Enable();
    }
    private void OnDisable()
    {
        //inputActions.Player.CandleToggle.performed -= OnToggleCandle;
        inputActions.Disable();
    }
    private void OnToggleCandle(InputAction.CallbackContext context)
    {
        Debug.Log("CandleToggled!");
        ToggleCandle();
    }
    public void ToggleCandle()
    {
        if (is_tweening_turning_off || is_tweening_turning_on) return;

        candle_turned_on = !candle_turned_on;
        if (candle_turned_on)
        {
            ActivateLight();
        }
        else
        {
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
        tween_start_position = transform.position;
        tween_time = 0f;
        is_tweening_turning_on = true;  // Start tweening when turning on
        time_counter = 0f;

        // Start flame scaling up
        if (flame_plane != null)
        {
            flame_plane.gameObject.SetActive(true); // Make visible first
            flame_plane.transform.localScale = new Vector3(0, original_flame_scale.y, original_flame_scale.z);
            flame_scale_time = 0f;
            is_flame_scaling_up = true;
            is_flame_scaling_down = false;
        }
    }
    private void DeactivateLight()
    {
        tween_start_position = transform.position;
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
        if (flame_plane == null) return;

        if (is_flame_scaling_up)
        {
            flame_scale_time += Time.deltaTime;
            float t = Mathf.Clamp01(flame_scale_time / flame_scale_duration);
            t = t * t * (3f - 2f * t); // Smoothstep

            float current_x_scale = Mathf.Lerp(0, original_flame_scale.x, t);
            flame_plane.transform.localScale = new Vector3(current_x_scale, original_flame_scale.y, original_flame_scale.z);

            // Fade lights in
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

            // Fade emission in
            if (candle_material != null)
            {
                float current_emission = Mathf.Lerp(emission_intensity_off, emission_intensity_on, t);
                candle_material.SetColor("_EmissionColor", base_emission_color * current_emission);
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
            t = t * t * (3f - 2f * t); // Smoothstep

            float current_x_scale = Mathf.Lerp(original_flame_scale.x, 0, t);
            flame_plane.transform.localScale = new Vector3(current_x_scale, original_flame_scale.y, original_flame_scale.z);

            // Fade lights out
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

            // Fade emission out
            if (candle_material != null)
            {
                float current_emission = Mathf.Lerp(emission_intensity_on, emission_intensity_off, t);
                candle_material.SetColor("_EmissionColor", base_emission_color * current_emission);
            }

            if (t >= 1f)
            {
                is_flame_scaling_down = false;
                flame_plane.gameObject.SetActive(false); // Make invisible when scale reaches 0
            }
        }
    }
    private void HandleCandlePosition()
    {
        if (is_tweening_turning_on)
        {
            // Tween to on position
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);
            t = t * t * (3f - 2f * t);
            starting_position = off_position + on_position_offset;
            Vector3 target_position = starting_position + on_position_offset;
            transform.position = Vector3.Lerp(tween_start_position, target_position, t);
            if (t >= 1f)
            {
                is_tweening_turning_on = false;
                starting_position = transform.position;  // Update starting position for oscillation
            }
        }
        else if (candle_turned_on && !is_tweening_turning_on)
        {
            time_counter += Time.deltaTime * floating_frequency_turned_on;
            float y_offset = Mathf.Sin(time_counter) * amplitude_movement_turned_on;
            transform.position = starting_position + new Vector3(0, y_offset, 0);
        }
        else if (is_tweening_turning_off)
        {
            // Tween to off position
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(tween_start_position, off_position, t);
            if (t >= 1f)
            {
                is_tweening_turning_off = false;
            }
        }
    }
}