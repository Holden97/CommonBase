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

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="pitch">音高</param>
        public void SetSound(SoundItem soundItem, float pitch)
        {
            audioSource.pitch = pitch;
            audioSource.volume = soundItem.soundVolume;
            audioSource.clip = soundItem.soundClip;
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="pitch">音高</param>
        public void SetSound(AudioClip clip, float pitch)
        {
            audioSource.pitch = pitch;
            audioSource.volume = 1;
            audioSource.clip = clip;
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

