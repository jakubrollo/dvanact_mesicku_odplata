using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Heavily vibe coded XD
/// Simple free-fly camera (WASD movement + mouse look).
/// - WASD : move
/// - Left Shift : accelerate (stacking while held)
/// - Space : move only on X/Z (preserve Y)
/// </summary>
[DisallowMultipleComponent]
public class FlyCam_NewInputSystem : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float mainSpeed = 30f;
    [SerializeField] private float shiftAdd = 50f;
    [SerializeField] private float maxShift = 1000f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.25f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool lockCursor = true;

    private float accumulatedRun = 1f;
    private float rotationX;
    private float rotationY;

    private void Start()
    {
        var angles = transform.eulerAngles;
        rotationY = angles.y;
        rotationX = angles.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        // scale sensitivity
        float mouseX = delta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = delta.y * mouseSensitivity * Time.deltaTime;

        rotationY += mouseX * 100f;
        rotationX += (invertY ? mouseY : -mouseY) * 100f;

        rotationX = Mathf.Clamp(rotationX, -89f, 89f);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        Vector3 input = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) input += Vector3.back;
        if (Keyboard.current.aKey.isPressed) input += Vector3.left;
        if (Keyboard.current.dKey.isPressed) input += Vector3.right;

        bool shiftHeld =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        if (shiftHeld)
        {
            accumulatedRun += Time.deltaTime;
            input *= accumulatedRun * shiftAdd;

            input.x = Mathf.Clamp(input.x, -maxShift, maxShift);
            input.z = Mathf.Clamp(input.z, -maxShift, maxShift);
        }
        else
        {
            accumulatedRun = Mathf.Clamp(accumulatedRun * 0.5f, 1f, 1000f);
            input *= mainSpeed;
        }

        Vector3 move = input * Time.deltaTime;

        bool holdSpace = Keyboard.current.spaceKey.isPressed;

        if (holdSpace)
        {
            float y = transform.position.y;
            transform.Translate(move, Space.Self);
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }
        else
        {
            transform.Translate(move, Space.Self);
        }
    }
}