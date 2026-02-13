using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using StarterAssets; // Potøebujeme pro StarterAssetsInputs

public class CandleBehavior : MonoBehaviourPun
{
    [Header("References")]
    [Tooltip("SEM pøetáhni objekt svíèky (to dítì, které se má hýbat)")]
    public Transform candleRoot;

    // Configuration Fields
    public bool candle_turned_on = false;
    public float amplitude_movement_turned_on = 0.05f;
    public float floating_frequency_turned_on = 2f;

    public Vector3 off_local_position;
    public float tween_duration = 0.5f;
    public Vector3 on_local_position_offset = new Vector3(0, 0.1f, 0);

    public GameObject flame_plane;
    public float flame_scale_duration = 0.5f;
    public Light[] flame_lights;

    public CandleAudio audio_script;

    public float emission_intensity_on = 1f;
    public float emission_intensity_off = 0f;

    // --- ZMÌNA: Používáme StarterAssetsInputs ---
    private StarterAssetsInputs _input; // Místo InputSystem_Actions

    private float time_counter = 0f;
    private Vector3 starting_local_position;
    private Vector3 tween_start_local_position;
    private Material runtimeMaterialInstance;
    private float tween_time = 0f;
    private bool is_tweening_turning_on = false;
    private bool is_tweening_turning_off = false;
    private float flame_scale_time = 0f;
    private bool is_flame_scaling_up = false;
    private bool is_flame_scaling_down = false;
    private Vector3 original_flame_scale;
    private float[] original_light_intensities;
    private Color base_emission_color;

    private void Awake()
    {
        // --- ZMÌNA: Získání reference na inputy ---
        _input = GetComponent<StarterAssetsInputs>();
        if (_input == null) Debug.LogError("CandleBehavior: Nenalezen StarterAssetsInputs na hráèi!");

        if (candleRoot == null)
        {
            Debug.LogError("CandleBehavior: CHYBÍ REFERENCE NA 'Candle Root'!");
            return;
        }

        if (audio_script == null) audio_script = candleRoot.GetComponent<CandleAudio>();

        off_local_position = candleRoot.localPosition;
        starting_local_position = off_local_position;

        Renderer candleRenderer = candleRoot.GetComponent<Renderer>();
        if (candleRenderer != null)
        {
            runtimeMaterialInstance = candleRenderer.material;
        }

        if (flame_plane != null)
        {
            original_flame_scale = flame_plane.transform.localScale;
            if (!candle_turned_on) flame_plane.gameObject.SetActive(false);
        }

        if (flame_lights != null && flame_lights.Length > 0)
        {
            original_light_intensities = new float[flame_lights.Length];
            for (int i = 0; i < flame_lights.Length; i++)
            {
                if (flame_lights[i] != null)
                {
                    original_light_intensities[i] = flame_lights[i].intensity;
                    if (!candle_turned_on) flame_lights[i].intensity = 0f;
                }
            }
        }

        if (runtimeMaterialInstance != null)
        {
            if (runtimeMaterialInstance.HasProperty("_EmissionColor"))
            {
                base_emission_color = runtimeMaterialInstance.GetColor("_EmissionColor");
                float maxComponent = Mathf.Max(base_emission_color.r, base_emission_color.g, base_emission_color.b);
                if (maxComponent > 0) base_emission_color /= maxComponent;
                runtimeMaterialInstance.SetColor("_EmissionColor", base_emission_color * (candle_turned_on ? emission_intensity_on : emission_intensity_off));
            }
        }
    }

    // --- ZMÌNA: OnEnable/OnDisable pro inputy už nepotøebujeme ---
    // Smazali jsme metody OnEnable, OnDisable a OnToggleCandle, protože input øešíme v Update

    private void Update()
    {
        if (candleRoot == null) return;

        // --- ZMÌNA: Kontrola inputu v Update ---
        if (photonView.IsMine && _input != null)
        {
            if (_input.candleToggle)
            {
                // Resetujeme input (aby se to necyklilo)
                _input.candleToggle = false;

                // Zavoláme RPC
                photonView.RPC("RPC_SetCandleState", RpcTarget.AllBuffered, !candle_turned_on);
            }
        }

        HandleCandlePosition();
        HandleFlameScale();
    }

    [PunRPC]
    public void RPC_SetCandleState(bool newState)
    {
        if (candle_turned_on == newState && !is_tweening_turning_off && !is_tweening_turning_on) return;
        if (is_tweening_turning_off || is_tweening_turning_on) return;

        candle_turned_on = newState;

        if (candle_turned_on)
        {
            if (audio_script) audio_script.TurnOn();
            ActivateLight();
        }
        else
        {
            if (audio_script) audio_script.TurnOff();
            DeactivateLight();
        }
    }

    // ... Zbytek metod (ActivateLight, DeactivateLight, HandleFlameScale, atd.) zùstává stejný ...
    // ... JEN SI JE TAM ZKOPÍRUJ Z PØEDCHOZÍHO KÓDU ...

    private void ActivateLight()
    {
        tween_start_local_position = candleRoot.localPosition;
        tween_time = 0f;
        is_tweening_turning_on = true;
        time_counter = 0f;

        if (flame_plane != null)
        {
            flame_plane.gameObject.SetActive(true);
            flame_plane.transform.localScale = new Vector3(0, original_flame_scale.y, original_flame_scale.z);
            flame_scale_time = 0f;
            is_flame_scaling_up = true;
            is_flame_scaling_down = false;
        }
    }

    private void DeactivateLight()
    {
        tween_start_local_position = candleRoot.localPosition;
        tween_time = 0f;
        is_tweening_turning_off = true;

        if (flame_plane != null)
        {
            flame_scale_time = 0f;
            is_flame_scaling_down = true;
            is_flame_scaling_up = false;
        }
    }

    private void HandleFlameScale()
    {
        if (flame_plane == null) return;

        if (is_flame_scaling_up)
        {
            flame_scale_time += Time.deltaTime;
            float t = Mathf.Clamp01(flame_scale_time / flame_scale_duration);
            t = t * t * (3f - 2f * t);

            float current_x_scale = Mathf.Lerp(0, original_flame_scale.x, t);
            flame_plane.transform.localScale = new Vector3(current_x_scale, original_flame_scale.y, original_flame_scale.z);

            UpdateLightsAndEmission(t, emission_intensity_off, emission_intensity_on, 0, 1);

            if (t >= 1f) is_flame_scaling_up = false;
        }
        else if (is_flame_scaling_down)
        {
            flame_scale_time += Time.deltaTime;
            float t = Mathf.Clamp01(flame_scale_time / flame_scale_duration);
            t = t * t * (3f - 2f * t);

            float current_x_scale = Mathf.Lerp(original_flame_scale.x, 0, t);
            flame_plane.transform.localScale = new Vector3(current_x_scale, original_flame_scale.y, original_flame_scale.z);

            UpdateLightsAndEmission(t, emission_intensity_on, emission_intensity_off, 1, 0);

            if (t >= 1f)
            {
                is_flame_scaling_down = false;
                flame_plane.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateLightsAndEmission(float t, float startEmission, float endEmission, float startLightRatio, float endLightRatio)
    {
        if (flame_lights != null)
        {
            for (int i = 0; i < flame_lights.Length; i++)
            {
                if (flame_lights[i] != null)
                {
                    float startIntensity = original_light_intensities[i] * startLightRatio;
                    float endIntensity = original_light_intensities[i] * endLightRatio;
                    flame_lights[i].intensity = Mathf.Lerp(startIntensity, endIntensity, t);
                }
            }
        }

        if (runtimeMaterialInstance != null)
        {
            float current_emission = Mathf.Lerp(startEmission, endEmission, t);
            runtimeMaterialInstance.SetColor("_EmissionColor", base_emission_color * current_emission);
        }
    }

    private void HandleCandlePosition()
    {
        Vector3 on_target_local_position = off_local_position + on_local_position_offset;

        if (is_tweening_turning_on)
        {
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);
            t = t * t * (3f - 2f * t);
            candleRoot.localPosition = Vector3.Lerp(tween_start_local_position, on_target_local_position, t);

            if (t >= 1f)
            {
                is_tweening_turning_on = false;
                starting_local_position = candleRoot.localPosition;
            }
        }
        else if (candle_turned_on)
        {
            time_counter += Time.deltaTime * floating_frequency_turned_on;
            float y_offset = Mathf.Sin(time_counter) * amplitude_movement_turned_on;
            candleRoot.localPosition = starting_local_position + new Vector3(0, y_offset, 0);
        }
        else if (is_tweening_turning_off)
        {
            tween_time += Time.deltaTime;
            float t = Mathf.Clamp01(tween_time / tween_duration);
            t = t * t * (3f - 2f * t);
            candleRoot.localPosition = Vector3.Lerp(tween_start_local_position, off_local_position, t);

            if (t >= 1f)
            {
                is_tweening_turning_off = false;
                candleRoot.localPosition = off_local_position;
            }
        }
        else
        {
            candleRoot.localPosition = off_local_position;
        }
    }
}