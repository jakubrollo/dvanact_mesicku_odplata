using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FlashlightFollow : MonoBehaviour
{
    RectTransform rt;
    Material mat;
    Image img;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();

        // Use 'material' (creates instance) instead of sharedMaterial
        // to avoid modifying the asset on disk or other objects
        mat = img.material;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, mousePos, null, out localPos);

        Vector2 uv = new Vector2(
            (localPos.x / rt.rect.width) + 0.5f,
            (localPos.y / rt.rect.height) + 0.5f
        );

        mat.SetVector("_Position", new Vector4(uv.x, uv.y, 0, 0));

        // FIX: Send Aspect Ratio to Shader to keep circle round
        float aspect = rt.rect.width / rt.rect.height;
        mat.SetFloat("_AspectRatio", aspect);
    }
}