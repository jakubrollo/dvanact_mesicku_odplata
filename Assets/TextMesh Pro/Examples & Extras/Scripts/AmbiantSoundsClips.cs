using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using System.Transactions;

public class AmbientClipsManager : MonoBehaviour
{
    public bool DISABLE_AMBIENTS = false;

    public AudioSource AmbientAudioSource;
    public AudioSource MusicAudioSource;
    public AudioSource WalkingAudioSource;
    public GameProgressionManager ProgressionManager;

    [Header("Hut Ambiance")]
    public AudioClip HutCampfire;
    public AudioClip Snoring;
    public AudioClip HutMusic;
    public AudioClip DoorSound;
    public AudioClip StepsHut;

    [Header("Forest Ambiance")]
    public AudioClip ForestAmbianceMusic;
    public AudioClip ForestAmbianceNature;
    public AudioClip ChirpingSound;
    public AudioClip GhostSound;
    public AudioClip StepsForest;

    private string scene = "";
    private float randomSoundTimer = 0f;
    private bool startedWalkingSounds = false;

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
        // Scene changed
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
        Debug.Log("scenename: " + sceneName);

        switch (sceneName)
        {
            case "Forest":
                CloseDoorSound();
                AmbientAudioSource.clip = ForestAmbianceNature;
                AmbientAudioSource.Play();
                MusicAudioSource.clip = ForestAmbianceMusic;
                MusicAudioSource.Play();
                break;
            case "Cabin":
                Debug.Log("got cabinnned lol");
                CloseDoorSound();
                AmbientAudioSource.clip = HutCampfire;
                AmbientAudioSource.Play();
                MusicAudioSource.clip = HutMusic;
                MusicAudioSource.Play();
                break;
            default:
                CloseDoorSound();
                AmbientAudioSource.clip = ForestAmbianceNature;
                AmbientAudioSource.Play();
                MusicAudioSource.clip = ForestAmbianceMusic;
                MusicAudioSource.Play();
                break;
        }
    }

    public void CloseDoorSound()
    {
        print("CloseDoorSound");
        if (DISABLE_AMBIENTS) return;
        AmbientAudioSource.PlayOneShot(DoorSound);
    }

    private void WalkingSound()
    {
        if (WalkingAudioSource == null) return;

        if (!WalkingAudioSource) return;

        var kb = Keyboard.current;
        if ((kb.wKey.isPressed ||
               kb.aKey.isPressed ||
               kb.sKey.isPressed ||
               kb.dKey.isPressed) && !kb.spaceKey.isPressed)
        {
            if (startedWalkingSounds)
                return;
            WalkingAudioSource.clip = scene == "Cabin" ? StepsHut : StepsForest;
            WalkingAudioSource.loop = true;
            WalkingAudioSource.loop = true;
            if (kb.shiftKey.isPressed)
            {
                WalkingAudioSource.pitch = 2f;
            }
            else
            {
                WalkingAudioSource.pitch = 1f;
            }
            WalkingAudioSource.Play();
            startedWalkingSounds = true;
        }
        else
        {
            if (WalkingAudioSource.isPlaying)
            {
                WalkingAudioSource.Stop();
                startedWalkingSounds = false;
            }
        }
    }

void Start()
    {
        // Tyto dva MUSÍ být přiřazené v Inspectoru (objekty ve scéně)
        if (MusicAudioSource != null) MusicAudioSource.loop = true;
        if (AmbientAudioSource != null) AmbientAudioSource.loop = true;
    }

    void Update()
    {
        // Hudba a Ambient jedou vždy (jsou ve scéně)
        if (MusicAudioSource != null && AmbientAudioSource != null)
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

        // Kroky řešíme jen když máme hráče
        if (WalkingAudioSource != null)
        {
            WalkingSound();
        }
    }
}
