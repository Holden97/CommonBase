using CommonBase;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(AudioSource))]
    public class Sound : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void SetSound(SoundItem soundItem, float pitch)
        {
            audioSource.pitch = pitch;
            audioSource.volume = soundItem.soundVolume;
            audioSource.clip = soundItem.soundClip;
        }

        public void Play()
        {
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }

        public void Stop() { audioSource.Stop(); }
    }
}

