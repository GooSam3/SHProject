using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMessagePopupDefault : ZUIFrameBase
{
    private Action mEventClickOk;
    private Action mEventClickCancel;

    [SerializeField]
    private Text TextDesc;

    [SerializeField]
    private Button btnCancel;

    public override bool IsDuplicatable => true;


    public void Set(string desc, Action onClickOk, Action onClickCancel)
    {
        TextDesc.text = desc;
        mEventClickOk = onClickOk;
        mEventClickCancel = onClickCancel;

        btnCancel.gameObject.SetActive(true);
    }

    public void Set(string desc, Action onClickOk)
    {
        TextDesc.text = desc;
        mEventClickOk = onClickOk;

        btnCancel.gameObject.SetActive(false);
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
        UIManager.Instance.Close<UIMessagePopupDefault>();
    }
}