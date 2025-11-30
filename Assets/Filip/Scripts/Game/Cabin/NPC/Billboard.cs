using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    [Header("Billboard")]
    [SerializeField] private Transform player; // odkaz na hr·Ëe nebo kameru

    [Header("Shitty Animation")]
    [SerializeField] private bool animateSprites = false;
    [SerializeField] private Sprite defaultS;
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;
    [SerializeField] private float switchInterval = 0.3f;

    private SpriteRenderer sr;
    private Coroutine animRoutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (animateSprites) StartAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 direction = -(player.position - transform.position);
        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    // --- PUBLIC METODY ---

    public void SetSprite(Sprite s)
    {
        StopAnimation();
        animateSprites = false;
        sr.sprite = s;
    }

    public void StartAnimation()
    {
        if (animRoutine != null) return;
        animateSprites = true;
        animRoutine = StartCoroutine(SwitchSprites());
    }

    public void StopAnimation()
    {
        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }
        animateSprites = false;
        sr.sprite = defaultS;
    }

    // --- COROUTINA ---

    private IEnumerator SwitchSprites()
    {
        bool toggle = false;

        while (true)
        {
            sr.sprite = toggle ? spriteA : spriteB;
            toggle = !toggle;

            yield return new WaitForSeconds(switchInterval);
        }
    }
}
