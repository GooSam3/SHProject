using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMessagePopupCost : ZUIFrameBase
{
    private Action mEventClickOk;
    private Action mEventClickCancel;

    [SerializeField]
    private Text TextTitle;
    [SerializeField]
    private Text TextDesc;
    [SerializeField]
    private Text TextCost;
    [SerializeField]
    private Image ImgCostIcon;

    [SerializeField]
    private Text TextCostDesc;// default : 소비금액

    [SerializeField]
    private ScrollRect TextScroller;

    public void Set(string title, string desc, uint costItemTid, ulong cost, Action onClickOk, Action onClickCancel, string costDescKey = "Cost_Desc")
    {
        TextScroller.normalizedPosition = Vector2.one;

        TextTitle.text = title;
        TextDesc.text = desc;
        TextCost.text = cost.ToString();

        ImgCostIcon.sprite = UICommon.GetItemIconSprite(costItemTid);

        TextCostDesc.text = DBLocale.GetText(costDescKey);

        mEventClickOk = onClickOk;
        mEventClickCancel = onClickCancel;
    }

    public void OnClickOk()
    {
        mEventClickOk?.Invoke();
        OnClickClose();
    }

    public void OnClickCancel()
    {
        mEventClickCancel?.Invoke();
        OnClickClose();
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close<UIMessagePopupCost>();
    }
}