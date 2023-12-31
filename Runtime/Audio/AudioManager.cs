﻿using CommonBase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CommonBase
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        public SoundSetting soundSetting;
        public Dictionary<int, AnimalSoundState> animalSoundDic;
        [SerializeField]
        GameObject soundPrefab;

        [Header("Audio Source")]
        [SerializeField] private AudioSource bgmAudioSource = null;

        [Header("Audio Mixers")]
        [SerializeField] private AudioMixer gameAudioMixer = null;


        private Dictionary<string, SoundItem> soundDictionary;

        [SerializeField]
        private SO_SoundList so_soundList = null;

        public AudioMixer AM => this.gameAudioMixer;


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

        /// <summary>
        /// 设置音量，不要在Awake中调用
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="v"></param>
        public void SetVolume(AudioChannelType channel, float v)
        {
            float volume = GetVolume(v);
            switch (channel)
            {
                case AudioChannelType.Overall:
                    this.soundSetting.overallVolume = v;
                    this.gameAudioMixer.SetFloat("Master", volume);
                    break;
                case AudioChannelType.BgmVolume:
                    this.soundSetting.bgmVolume = v;
                    this.gameAudioMixer.SetFloat("Bgm", volume);
                    break;
                case AudioChannelType.efxVolume:
                    this.soundSetting.efxVolume = v;
                    this.gameAudioMixer.SetFloat("Effect", volume);
                    break;
                case AudioChannelType.gameVolume:
                    this.soundSetting.gameVolume = v;
                    this.gameAudioMixer.SetFloat("Game", volume);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取音量对应分贝
        /// </summary>
        /// <param name="v">音量，0-1</param>
        /// <returns></returns>
        private static float GetVolume(float v)
        {
            var volume = -80f;
            if (v > 0)
            {
                volume = v * 30 - 20;
            }

            return volume;
        }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
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

            ObjectPoolManager.Instance.CreatePool("sound", soundPrefab, 10, dontDestroyOnLoad: true);

            ;

            soundSetting = new SoundSetting(
                PlayerPrefs.GetFloat("MasterVolume", 0.6f),
                PlayerPrefs.GetFloat("GameVolume", 0.6f),
                PlayerPrefs.GetFloat("BgmVolume", 0.5f),
                PlayerPrefs.GetFloat("EffectVolume", 0.6f)
                );

            this.PlayBgm("Bgm2");
        }

        public void OnDestroyManager()
        {
            PlayerPrefs.SetFloat("MasterVolume", this.soundSetting.overallVolume);
            PlayerPrefs.SetFloat("GameVolume", this.soundSetting.gameVolume);
            PlayerPrefs.SetFloat("BgmVolume", this.soundSetting.bgmVolume);
            PlayerPrefs.SetFloat("EffectVolume", this.soundSetting.efxVolume);

            PlayerPrefs.Save();
        }

        private void Start()
        {
            SetVolume(AudioChannelType.Overall, soundSetting.overallVolume);
            SetVolume(AudioChannelType.BgmVolume, soundSetting.bgmVolume);
            SetVolume(AudioChannelType.efxVolume, soundSetting.efxVolume);
            SetVolume(AudioChannelType.gameVolume, soundSetting.gameVolume);
        }

        public Sound PlaySound(string soundName, float pitch = 1f, Action OnDone = null, bool loop = false)
        {
            if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
            {
                return SetSound(pitch, OnDone, loop, soundItem);
            }
            return null;
        }

        public Sound PlaySound(AudioClip clip, float pitch = 1f, Action OnDone = null, bool loop = false)
        {
            if (clip == null) return null;
            GameObject soundGameObject = ObjectPoolManager.Instance.GetNextObject("sound");
            Sound sound = soundGameObject.GetOrAddComponent<Sound>();
            sound.SetSound(clip, pitch);
            soundGameObject.SetActive(true);
            if (loop)
            {
                sound.Loop = true;
            }
            else
            {
                StartCoroutine(DisableSound(sound, clip.length, OnDone));
            }
            sound.Play();
            return sound;
        }

        private Sound SetSound(float pitch, Action OnDone, bool loop, SoundItem soundItem)
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
            if (soundName == null) return;
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

        public void Pause()
        {

        }

        public void Resume()
        {

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

    [Serializable]
    public class SoundSetting
    {
        public float overallVolume;
        public float gameVolume;
        public float bgmVolume;
        public float efxVolume;

        public SoundSetting(float overallVolume, float gameVolume, float bgmVolume, float efxVolume)
        {
            this.overallVolume = overallVolume;
            this.gameVolume = gameVolume;
            this.bgmVolume = bgmVolume;
            this.efxVolume = efxVolume;
        }
    }

    public enum AudioChannelType
    {
        Overall,
        BgmVolume,
        efxVolume,
        gameVolume,
    }
}
