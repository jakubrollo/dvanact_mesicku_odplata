using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Look")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 15f; // reduced scale for New Input
    [SerializeField] private float lookXLimit = 85f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Calculate Movement Input (WASD)
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Polling keys directly from New Input System
        float curSpeedX = 0;
        float curSpeedY = 0;

        if (Keyboard.current != null)
        {
            bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
            float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Y is Forward/Back, X is Left/Right in Input mapping
            float inputY = 0;
            float inputX = 0;

            if (Keyboard.current.wKey.isPressed) inputY = 1;
            if (Keyboard.current.sKey.isPressed) inputY = -1;
            if (Keyboard.current.aKey.isPressed) inputX = -1;
            if (Keyboard.current.dKey.isPressed) inputX = 1;

            curSpeedX = targetSpeed * inputY;
            curSpeedY = targetSpeed * inputX;
        }

        // 2. Apply Movement
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // 3. Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y = movementDirectionY - (gravity * Time.deltaTime);
        }
        else
        {
            moveDirection.y = -2f; // Stick to ground
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // 4. Camera Rotation (Mouse Look)
        if (Mouse.current != null)
        {
            // New Input gives very high delta values, so we multiply by Time.deltaTime
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            rotationX += -mouseDelta.y * mouseSensitivity * Time.deltaTime;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            if (playerCamera)
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            transform.rotation *= Quaternion.Euler(0, mouseDelta.x * mouseSensitivity * Time.deltaTime, 0);
        }
    }
}