using UnityEngine;
// using Photon.Pun; // Photon už není potøeba pro filtrování, chceme vidìt všechna svìtla

[RequireComponent(typeof(SpriteRenderer))]
public class PathReflector : MonoBehaviour
{
    [Header("Glare Settings")]
    [SerializeField] private Color glimmerColor = Color.cyan;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float glareFocus = 8.0f;
    [SerializeField] private float shimmerSpeed = 3.0f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInSpeed = 2.0f;
    [SerializeField] private float fadeOutSpeed = 5.0f;

    [Header("Light Settings")]
    [SerializeField] private Light pointLight;
    [SerializeField] private float maxLightIntensity = 3.0f;

    // --- ZMÌNA: Místo jedné svíèky jich ukládáme pole ---
    private CandleBehavior[] allCandles;
    private float searchTimer = 0f; // Èasovaè pro hledání nových hráèù

    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Init colors
        Color c = glimmerColor;
        c.a = 0f;
        spriteRenderer.color = c;
        if (pointLight) pointLight.intensity = 0f;

        // První hledání ihned
        RefreshCandles();
    }

    void Update()
    {
        // --- 1. PERIODICKÉ HLEDÁNÍ ---
        // Každou vteøinu zkontrolujeme, jestli se nìkdo nepøipojil/neodpojil
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            RefreshCandles();
            searchTimer = 1.0f; // Další hledání za vteøinu
        }

        // --- 2. VÝPOÈET NEJSILNÌJŠÍHO SVÌTLA ---
        float maxDotProduct = 0f;
        bool anyCandleActive = false;

        if (allCandles != null)
        {
            foreach (var candle in allCandles)
            {
                // Pøeskoèíme znièené objekty (odpojení hráèi)
                if (candle == null) continue;

                // Pøeskoèíme zhasnuté svíèky
                if (!candle.candle_turned_on) continue;

                anyCandleActive = true;

                // Spoèítáme, jak moc tato konkrétní svíèka míøí na reflektor
                Vector3 directionToReflector = (transform.position - candle.transform.position).normalized;
                float dot = Vector3.Dot(candle.transform.forward, directionToReflector);
                //Debug.Log($"dot product:{dot}");
                // Hledáme maximální hodnotu ze všech hráèù
                if (dot > maxDotProduct)
                {
                    maxDotProduct = dot;
                }
            }
        }

        // --- 3. VÝPOÈET CÍLOVÉ ALFY ---
        float targetAlpha = 0f;

        if (anyCandleActive && maxDotProduct > 0f)
        {
            float glareIntensity = Mathf.Pow(maxDotProduct, glareFocus);

            // Šum pro efekt tøpycení
            float shimmer = Mathf.PerlinNoise(Time.time * shimmerSpeed, 0f) * 0.2f + 0.8f;

            targetAlpha = maxAlpha * glareIntensity * shimmer;
        }

        // --- 4. APLIKACE (Lerp, Barva, Svìtlo) ---
        // (Zbytek kódu je stejný jako pøedtím)

        float speed = (targetAlpha > currentAlpha) ? fadeInSpeed : fadeOutSpeed;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * speed);

        Color finalColor = glimmerColor;
        finalColor.a = currentAlpha;
        spriteRenderer.color = finalColor;
        spriteRenderer.enabled = currentAlpha > 0.01f;

        if (pointLight != null)
        {
            float percent = currentAlpha / maxAlpha;
            float easeInCurve = percent * percent;
            pointLight.intensity = easeInCurve * maxLightIntensity;
            pointLight.enabled = currentAlpha > 0.001f;
        }
    }

    private void RefreshCandles()
    {
        // Find all candles
        allCandles = FindObjectsByType<CandleBehavior>(FindObjectsSortMode.None);

        // --- DEBUG LOG START ---
        string logMessage = $"[PathReflector] Found {allCandles.Length} candles: ";

        foreach (var candle in allCandles)
        {
            if (candle != null)
            {
                logMessage += $"[{candle.gameObject.name}] ";
            }
        }

        Debug.Log(logMessage); // Check your Console for this line
        // --- DEBUG LOG END ---
    }
}