using UnityEngine;

namespace CommonBase
{
    [RequireComponent(typeof(AudioSource))]
    public class Sound : MonoBehaviour
    {
        public bool Loop
        {
            get
            {
                return audioSource.loop;
            }
            set
            {
                audioSource.loop = value;
            }
        }
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

        internal void StopAudio() { audioSource.Stop(); }
    }
}

