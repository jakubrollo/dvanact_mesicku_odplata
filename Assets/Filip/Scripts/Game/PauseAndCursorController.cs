using UnityEngine;
using UnityEngine.InputSystem;

public class PauseAndCursorController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseActionReference;

    private InputAction pauseAction;
    private PlayerInput playerInput; // Cache this reference

    private void Awake()
    {
        // Cache the PlayerInput component so we don't use GetComponent every time we pause
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        // IMPORTANT: Ensure the menu is hidden and cursor is locked when the game starts
        if (pauseMenu != null) pauseMenu.SetActive(false);
        SetPauseState(false);
    }

    private void OnEnable()
    {
        if (pauseActionReference != null)
        {
            pauseAction = pauseActionReference.action;
            pauseAction.Enable();
            pauseAction.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        // Toggle the state based on the current state of the menu
        bool isCurrentlyPaused = pauseMenu.activeSelf;
        SetPauseState(!isCurrentlyPaused);
    }

    public void SetPauseState(bool isPaused)
    {
        // 1. Handle UI
        if (pauseMenu != null) pauseMenu.SetActive(isPaused);

        // 2. Handle Input Action Map (Switching between Gameplay and UI)
        if (playerInput != null)
        {
            if (isPaused)
            {
                // Option A: Disable player controls
                // playerInput.actions.FindActionMap("Player")?.Disable(); 

                // Option B (Better): Switch to a UI map if you have one
                playerInput.SwitchCurrentActionMap("UI");
            }
            else
            {
                // playerInput.actions.FindActionMap("Player")?.Enable();
                playerInput.SwitchCurrentActionMap("Player");
            }
        }

        // 3. Handle Time
        Time.timeScale = isPaused ? 0f : 1f;

        // 4. Handle Cursor (The part you asked about)
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None; // Unlocks cursor so you can click buttons
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Locks cursor to center
            Cursor.visible = false; // Hides cursor
        }
    }
}