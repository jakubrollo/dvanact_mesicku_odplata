using UnityEngine;
using UnityEngine.InputSystem;

public class CandleBehavior : MonoBehaviour
{

    public bool candle_turned_on = false;
    public float amplitude_movement_turned_on = 10f;
    public float floating_frequency_turned_on = 2f;
    public Vector3 off_position = Vector3.zero; // Position when candle is off
    public float tween_duration = 0.5f; // How long the tween takes

    private InputSystem_Actions inputActions;
    private Vector3 starting_position;
    private float time_counter = 0f;

    // For tweening
    private bool is_tweening = false;
    private Vector3 tween_start_position;
    private float tween_time = 0f;

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
        starting_position = transform.position;
        time_counter = 0f;
        is_tweening = false;
    }

    private void DeactivateLight()
    {
        tween_start_position = transform.position;
        tween_time = 0f;
        is_tweening = true;
    }

    private void HandleCandlePosition()
    {
        if (candle_turned_on && !is_tweening)
        {
            time_counter += Time.deltaTime * floating_frequency_turned_on;
            float y_offset = Mathf.Sin(time_counter) * amplitude_movement_turned_on;
            transform.position = starting_position + new Vector3(0, y_offset, 0);
        }
        else if (is_tweening)
        {
            // Tween to off position
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);

            // Smooth step for nicer easing
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(tween_start_position, off_position, t);

            if (t >= 1f)
            {
                is_tweening = false;
            }
        }
    }


}