using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMessagePopupNormal : ZUIFrameBase
{
    private Action<bool> mEventCheckClickOk;
    private Action mEventClickOk;
    private Action mEventClickCancel;

    [SerializeField]
    private Text TextTitle;
    [SerializeField]
    private Text TextDesc;
    [SerializeField]
    private ZToggle CheckToggle;
    [SerializeField]
    private Text TextCheckTitle;

    public void Set(string title, string desc, Action onClickOk, Action onClickCancel)
    {
        TextTitle.text = title;
        TextDesc.text = desc;
        mEventClickOk = onClickOk;
        mEventClickCancel = onClickCancel;

        CheckToggle.gameObject.SetActive(false);
    }

    public void Set(string title, string desc,string checktitle,bool defaultCheck ,Action<bool> onClickOk, Action onClickCancel)
    {
        TextTitle.text = title;
        TextDesc.text = desc;
        TextCheckTitle.text = checktitle;
        mEventCheckClickOk = onClickOk;
        mEventClickCancel = onClickCancel;

        CheckToggle.gameObject.SetActive(true);
        CheckToggle.isOn = defaultCheck;
    }

    public void OnClickOk()
    {
        if (CheckToggle.gameObject.activeSelf)
        {
            mEventCheckClickOk?.Invoke(CheckToggle.isOn);
        }
        else
        {
            mEventClickOk?.Invoke();
        }
        OnClickClose();
    }

    public void OnClickCancel()
    {
        mEventClickCancel?.Invoke();
        OnClickClose();
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close<UIMessagePopupNormal>();
    }
}