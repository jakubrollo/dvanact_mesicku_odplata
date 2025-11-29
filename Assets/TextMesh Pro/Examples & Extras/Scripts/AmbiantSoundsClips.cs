using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace TMPro.Examples
{

    public class SimpleScript : MonoBehaviour
    {
        public AudioSource AmbientAudioSource;

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

        public void PlayRandomForrestSound()
        {
            double GhostPropability = 0.3;
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

        void Start()
        { 
        }

        void Update()
        {

        }
    }
}
