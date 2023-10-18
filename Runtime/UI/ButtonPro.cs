using CommonBase;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPro : Button
{
    public ButtonType type;
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        this.transform.DOScale(1.05f, 0.2f).SetUpdate(true);
        switch (type)
        {
            case ButtonType.NORMAL:
                AudioManager.Instance.PlaySound("NormalBtnHover", 1);
                break;
            default:
                break;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        switch (type)
        {
            case ButtonType.NORMAL:
                AudioManager.Instance.PlaySound("NormalBtnClick", 1);
                break;
            default:
                break;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        this.transform.DOScale(0.9f, 0.2f).SetUpdate(true);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        this.transform.DOScale(1f, 0.2f).SetUpdate(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        this.transform.DOScale(1f, 0.2f).SetUpdate(true);
    }
}
