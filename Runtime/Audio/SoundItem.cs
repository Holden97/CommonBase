using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    [System.Serializable]
    public class SoundItem
    {
        public string soundName;
        public AudioClip soundClip;
        public string soundDescription;
        public bool randomPitch = false;
        [Range(0.1f, 1.5f)]
        public float soundPitchRandomVariationMin = 0.8f;
        [Range(0.1f, 1.5f)]
        public float soundPitchRandomVariationMax = 1.2f;
        [Range(0f, 1f)]
        public float soundVolume = 1f;
    }
}

