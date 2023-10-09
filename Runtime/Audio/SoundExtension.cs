using UnityEngine;

namespace CommonBase
{
    public static class SoundExtension
    {
        public static void Stop(this Sound sound)
        {
            if (sound == null) return;
            sound.StopAudio();
            AudioManager.Instance.Stop(sound);
        }
    }
}

