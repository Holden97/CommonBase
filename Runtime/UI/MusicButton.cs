using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonBase
{
    public class MusicButton : Button
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            AudioManager.Instance.PlaySound("Btn2", 0.3f);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            AudioManager.Instance.PlaySound("Btn2", 0.6f);
        }
    }
}
