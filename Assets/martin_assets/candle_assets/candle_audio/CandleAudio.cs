using UnityEngine;

public class CandleAudio : MonoBehaviour
{
    public AudioClip candleLightSound;  // Sound when turning on
    public AudioClip candleExtinguishSound;  // Sound when turning off
    public AudioClip candleExtinguishSound2;  // Sound when turning off
    public AudioClip candleLoopSound;  // Optional: ambient crackling while lit

    private AudioSource audioSource;
    private AudioSource loopAudioSource;  // Separate source for looping

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // If you want a looping sound, create a second AudioSource
        if (candleLoopSound != null)
        {
            loopAudioSource = gameObject.AddComponent<AudioSource>();
            loopAudioSource.clip = candleLoopSound;
            loopAudioSource.loop = true;
            loopAudioSource.playOnAwake = false;
        }
    }

    public void TurnOn()
    {
        audioSource.PlayOneShot(candleLightSound);

        if (loopAudioSource != null)
        {
            loopAudioSource.Play();
        }
    }

    public void TurnOff()
    {
        audioSource.PlayOneShot(candleExtinguishSound);
        //Debug.Log("candle extinguished");
        //Debug.Log(candleExtinguishSound);
        //audioSource.PlayOneShot(candleExtinguishSound2);

        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
        }
    }
}