using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCandle : MonoBehaviour
{
    public bool IsCandleOn { get; private set; }
    [SerializeField] private Light candleLight;

    void Update()
    {

        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (ePressed || mousePressed)
        {
            IsCandleOn = !IsCandleOn;
            if (candleLight) candleLight.enabled = IsCandleOn;
        }
    }
}