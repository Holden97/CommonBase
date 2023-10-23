using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonBase
{
    public class AudioSlider : MonoBehaviour
    {
        public Slider audioSlider;
        public Text audioVolumeTxt;
        public AudioChannelType channelType;

        public void OnVolumeChange(float d)
        {
            UpdateText(d);
            AudioManager.Instance.SetVolume(channelType, d);
        }

        private void UpdateText(float f)
        {
            audioVolumeTxt.text = ((int)(f * 100)).ToString() + "%";
        }

        public void UpdateView(float f)
        {
            this.audioSlider.value = f;
        }

    }
}

