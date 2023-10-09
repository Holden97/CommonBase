using CommonBase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CommonBase
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        public Dictionary<int, AnimalSoundState> animalSoundDic;
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


        public void RegisterAnimal(int animalId)
        {
            animalSoundDic.Add(animalId, new AnimalSoundState(animalId, false));
        }

        public void UnregisterAnimal(int animalId)
        {
            if (animalSoundDic.ContainsKey(animalId))
            {
                animalSoundDic.Remove(animalId);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            soundDictionary = new Dictionary<string, SoundItem>();

            foreach (var soundItem in so_soundList.soundDetails)
            {
                soundDictionary.Add(soundItem.soundName, soundItem);
            }
            this.animalSoundDic = new Dictionary<int, AnimalSoundState>();

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

        public Sound PlaySound(string soundName, float pitch = 1f, Action OnDone = null, bool loop = false)
        {
            if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
            {
                GameObject soundGameObject = ObjectPoolManager.Instance.GetNextObject("sound");
                Sound sound = soundGameObject.GetOrAddComponent<Sound>();
                sound.SetSound(soundItem, pitch);
                soundGameObject.SetActive(true);
                if (loop)
                {
                    sound.Loop = true;
                }
                else
                {
                    StartCoroutine(DisableSound(sound, soundItem.soundClip.length, OnDone));
                }
                sound.Play();
                return sound;
            }
            return null;
        }

        public void Stop(Sound item)
        {
            ObjectPoolManager.Instance.Putback("sound", item.gameObject);
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

        private IEnumerator DisableSound(Sound sound, float length, Action onDone = null)
        {
            yield return new WaitForSeconds(length);
            Stop(sound);
            onDone?.Invoke();
        }

        public void PlayAnimalSound(int animalId, string soundName, float pitch = 1f, bool forcePlay = false)
        {
            if (animalSoundDic.TryGetValue(animalId, out AnimalSoundState state))
            {
                if (!state.isMakingSound || forcePlay)
                {
                    state.isMakingSound = true;
                    PlaySound(soundName, pitch, () => state.isMakingSound = false);
                }
            }
            else
            {
                animalSoundDic.Add(animalId, new AnimalSoundState(animalId, false));
                animalSoundDic[animalId].isMakingSound = true;
                PlaySound(soundName, pitch, () => state.isMakingSound = false);
            }
        }
    }



    public class AnimalSoundState
    {
        public int ownerId;
        public bool isMakingSound;
        public GameObject curSound;

        public AnimalSoundState(int ownerId, bool isMakingSound)
        {
            this.ownerId = ownerId;
            this.isMakingSound = isMakingSound;
        }
    }

}
