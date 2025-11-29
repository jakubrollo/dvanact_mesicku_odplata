using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class AmbientClipsManager : MonoBehaviour
{
    public bool DISABLE_AMBIENTS = false;

    public AudioSource AmbientAudioSource;
    public AudioSource MusicAudioSource;
    public GameProgressionManager ProgressionManager;

    [Header("Hut Ambiance")]
    public AudioClip HutCampfire;
    public AudioClip Snoring;
    public AudioClip HutMusic;
    public AudioClip DoorSound;

    [Header("Forest Ambiance")]
    public AudioClip ForestAmbianceMusic;
    public AudioClip ForestAmbianceNature;
    public AudioClip ChirpingSound;
    public AudioClip GhostSound;

    private string scene = "";
    private float randomSoundTimer = 0f;

    public void PlayRandomForrestSound()
    {
        if (DISABLE_AMBIENTS) return;

        double GhostPropability = 0.6f;
        var rnd = new System.Random();

        if (rnd.NextDouble() < GhostPropability)
        {
            AmbientAudioSource.PlayOneShot(GhostSound);
        }
        else
        {
            AmbientAudioSource.PlayOneShot(ChirpingSound);
        }
    }

    public void RunAmbientMusicBasedOnScene(string sceneName)
    {
        if (DISABLE_AMBIENTS) return;
        if (sceneName == scene) 
            return;
        print("RunAmbientMusicBasedOnScene");

        scene = sceneName;
        if (AmbientAudioSource.isPlaying)
        {
            AmbientAudioSource.Stop();
        }
        if (MusicAudioSource.isPlaying)
        {
            MusicAudioSource.Stop();
        }

        MusicAudioSource.loop = true;
        AmbientAudioSource.loop = true;

        switch (sceneName)
        {
            case "Forest":
                AmbientAudioSource.clip = ForestAmbianceNature;
                AmbientAudioSource.Play();
                MusicAudioSource.clip = ForestAmbianceMusic;
                MusicAudioSource.Play();
                break;
            case "Cabin":
                AmbientAudioSource.clip = HutCampfire;
                AmbientAudioSource.Play();
                MusicAudioSource.clip = HutMusic;
                MusicAudioSource.Play();
                break;
            default: break;
        }
    }

    public void CloseDoorSound()
    {
        print("CloseDoorSound");
        if (DISABLE_AMBIENTS) return;
        AmbientAudioSource.PlayOneShot(DoorSound);
    }

    void Start()
    { 
        MusicAudioSource.loop = true;
        AmbientAudioSource.loop = true;
    }

    void Update()
    {
        RunAmbientMusicBasedOnScene(ProgressionManager.CurrentSceneName());

        if (scene == "Forest")
        {
            randomSoundTimer += Time.deltaTime;

            if (randomSoundTimer >= 22f)
            {
                print("Play random sound!");
                randomSoundTimer = 0f;  
                PlayRandomForrestSound();  
            }
        }
    }
}
