using UnityEngine;

public class CandleAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip candleLightSound;       // Zvuk zapálení
    public AudioClip candleExtinguishSound;  // Zvuk zhasnutí
    public AudioClip candleLoopSound;        // Smyèka hoøení (praskání)

    [Header("Audio Configuration")]
    [Tooltip("Pokud je prázdné, skript si ho najde na tomto objektu.")]
    public AudioSource audioSource;

    private AudioSource loopAudioSource;  // Druhý zdroj pro smyèku

    void Awake()
    {
        // 1. Automatické nalezení nebo pøidání AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // 2. DÙLEŽITÉ PRO MULTIPLAYER: Vynutit 3D zvuk
        // Pokud by toto bylo 0, slyšel bys všechny hráèe stejnì hlasitì všude po mapì.
        audioSource.spatialBlend = 1.0f;
        audioSource.playOnAwake = false;

        // 3. Vytvoøení zdroje pro smyèku (pokud existuje klip)
        if (candleLoopSound != null)
        {
            loopAudioSource = gameObject.AddComponent<AudioSource>();
            loopAudioSource.clip = candleLoopSound;
            loopAudioSource.loop = true;
            loopAudioSource.playOnAwake = false;

            // Zkopírujeme nastavení z hlavního zdroje, aby se chovaly stejnì
            loopAudioSource.spatialBlend = 1.0f; // Taky musí být 3D!
            loopAudioSource.rolloffMode = audioSource.rolloffMode;
            loopAudioSource.minDistance = audioSource.minDistance;
            loopAudioSource.maxDistance = audioSource.maxDistance;
            loopAudioSource.volume = audioSource.volume * 0.8f; // Smyèka bývá trochu tišší
        }
    }

    public void TurnOn()
    {
        if (candleLightSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(candleLightSound);
        }

        if (loopAudioSource != null)
        {
            if (!loopAudioSource.isPlaying) loopAudioSource.Play();
        }
    }

    public void TurnOff()
    {
        if (candleExtinguishSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(candleExtinguishSound);
        }

        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
        }
    }
}