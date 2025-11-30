using UnityEngine;
using UnityEngine.InputSystem;

public class PauseAndCursorController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseActionReference; // odkaz na tlaèítko ESC / Pause

    private InputAction pauseAction; // uložíme si akci lokálnì

    private void OnEnable()
    {
        if (pauseActionReference != null)
        {
            pauseAction = pauseActionReference.action; // uložíme akci
            pauseAction.Enable();                    // povolíme
            pauseAction.performed += OnPausePerformed; // pøipojíme listener
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed; // odpojíme listener
            pauseAction.Disable();                      // zakážeme akci
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        bool active = !pauseMenu.activeSelf;
        pauseMenu.SetActive(active);

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var playerMap = playerInput.actions.FindActionMap("Player");
            if (active)
                playerMap.Disable();
            else
                playerMap.Enable();
        }

        Time.timeScale = active ? 0f : 1f;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = active;
    }
}
