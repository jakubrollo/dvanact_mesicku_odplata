using UnityEngine;
using UnityEngine.InputSystem;

public class LunarDudesTrigger : MonoBehaviour
{

    [Tooltip("Drag the Enemy object here so we can turn him off")]
    private bool hasTriggered = false;
    [SerializeField] private LunarDudesController dudes;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            dudes.ActivateLunarDudesCutscene(GameProgressionManager.Instance.GetCurrentLevelData().ForestStage);
        }
    }


}
