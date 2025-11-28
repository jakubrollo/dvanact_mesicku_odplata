using UnityEngine;
using UnityEngine.UI;

public class UIFadeIn : MonoBehaviour
{
    public float speed = 1f;
    public Image img;
    void Awake()
    {
        img.raycastTarget = false;
    }
    void Update()
    {
        img.color = Color.Lerp(img.color, new Color(0, 0, 0, 0), Time.deltaTime * speed);
    }
}
