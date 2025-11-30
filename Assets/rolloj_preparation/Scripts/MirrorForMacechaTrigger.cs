using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorForMacechaTrigger : MonoBehaviour
{

    [Tooltip("Drag the Enemy object here so we can turn him off")]
    private bool hasTriggered = false;
    [SerializeField] private FirstStageManager firstStageManager;
    [SerializeField] private CandleBehavior candleBehavior;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && candleBehavior.candle_turned_on)
        {
            hasTriggered = true;
            firstStageManager.PlayerFoundMirror = true;
        }
    }

    
}
