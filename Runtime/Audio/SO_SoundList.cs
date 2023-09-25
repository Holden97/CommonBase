using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    [CreateAssetMenu(fileName = "so_SoundList", menuName = "Scriptable Object/Sound/Sound List")]
    public class SO_SoundList : ScriptableObject
    {
        [SerializeField]
        public List<SoundItem> soundDetails;
    }
}

