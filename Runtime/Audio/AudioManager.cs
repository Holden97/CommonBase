using CommonBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CommonBase
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [SerializeField]
        GameObject soundPrefab;

        [Header("Audio Source")]
        [SerializeField] private AudioSource bgmAudioSource = null;
        [SerializeField] private AudioSource gameMusicAudioSource = null;

        [Header("Audio Mixers")]
        [SerializeField] private AudioMixer gameAudioMixer = null;

        [Header("Audio Snapshots")]
        [SerializeField] private AudioMixerSnapshot gameMusicSnapshot = null;
        [SerializeField] private AudioMixerSnapshot gameAmbientSnapshot = null;


        private Dictionary<string, SoundItem> soundDictionary;

        [SerializeField]
        private SO_SoundList so_soundList = null;

        public override void Initialize()
        {
            base.Initialize();
            soundDictionary = new Dictionary<string, SoundItem>();

            foreach (var soundItem in so_soundList.soundDetails)
            {
                soundDictionary.Add(soundItem.soundName, soundItem);
            }

            ObjectPoolManager.Instance.CreatePool(10, soundPrefab, "sound");

            this.PlayBgm("Bgm2");
        }

        private void PlayMusicSoundClip(SoundItem musicSoundItem, float musicTransitionSecs)
        {
            gameAudioMixer.SetFloat("MusicVolume", ConvertSoundVolumeDecimalFractionToDecibels(musicSoundItem.soundVolume));

            gameMusicAudioSource.clip = musicSoundItem.soundClip;
            gameMusicAudioSource.Play();

            gameMusicSnapshot.TransitionTo(musicTransitionSecs);
        }

        private void PlayBgmSoundClip(SoundItem musicSoundItem, float musicTransitionSecs)
        {
            gameAudioMixer.SetFloat("MusicVolume", ConvertSoundVolumeDecimalFractionToDecibels(musicSoundItem.soundVolume));

            gameMusicAudioSource.clip = musicSoundItem.soundClip;
            gameMusicAudioSource.Play();

            gameMusicSnapshot.TransitionTo(musicTransitionSecs);
        }

        private void PlayAmbientSoundClip(SoundItem ambientSoundItem, float transitionTimeSeconds)
        {
            gameAudioMixer.SetFloat("AmbientVolume", ConvertSoundVolumeDecimalFractionToDecibels(ambientSoundItem.soundVolume));

            bgmAudioSource.clip = ambientSoundItem.soundClip;
            bgmAudioSource.Play();

            gameAmbientSnapshot.TransitionTo(transitionTimeSeconds);
        }

        private float ConvertSoundVolumeDecimalFractionToDecibels(float soundVolume)
        {
            return (soundVolume * 100f - 80f);
        }

        public void PlaySound(string soundName, float pitch = 0.5f)
        {
            if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
            {
                GameObject soundGameObject = ObjectPoolManager.Instance.GetNextObject("sound");
                Sound sound = soundGameObject.GetOrAddComponent<Sound>();
                sound.SetSound(soundItem, pitch);
                soundGameObject.SetActive(true);
                sound.Play();
                StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));
            }
        }

        public void PlayBgm(string bgmName)
        {
            if (soundDictionary.TryGetValue(bgmName, out SoundItem soundItem))
            {
                this.bgmAudioSource.clip = soundItem.soundClip;
                this.bgmAudioSource.loop = true;
                this.bgmAudioSource.Play();
            }
        }

        private IEnumerator DisableSound(GameObject soundGameObject, float length)
        {
            yield return new WaitForSeconds(length);
            ObjectPoolManager.Instance.Putback("sound", soundGameObject);
        }
    }

}
