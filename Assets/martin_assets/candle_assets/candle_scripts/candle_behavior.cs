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

    private InputSystem_Actions inputActions;
    private Vector3 starting_position;
    private float time_counter = 0f;

    // For tweening
    
    private Vector3 tween_start_position;
    private float tween_time = 0f;

    private bool is_tweening_turning_on = false;
    private bool is_tweening_turning_off = false;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        starting_position = transform.position;
        off_position = transform.position;


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
    }

    private void ActivateLight()
    {
        tween_start_position = transform.position;
        tween_time = 0f;
        is_tweening_turning_on = true;  // Start tweening when turning on
        time_counter = 0f;
    }

    private void DeactivateLight()
    {
        tween_start_position = transform.position;
        tween_time = 0f;
        is_tweening_turning_off = true;
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