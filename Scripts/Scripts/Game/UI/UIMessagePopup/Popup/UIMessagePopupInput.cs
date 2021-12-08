using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMessagePopupInput : ZUIFrameBase
{
    private Action<string> mEventClickOk;
    private Action mEventClickCancel;

    [SerializeField]
    private InputField InputName;
	[SerializeField]
	private Text TextInputHolder;
	[SerializeField]
    private Text TextTitle;
    [SerializeField]
    private Text TextDesc;

	public void Set(string title, string desc, Action<string> onClickOk, Action onClickCancel, int limit)
    {
        InputName.text = "";
        TextTitle.text = title;

        InputName.characterLimit = limit;
        TextDesc.text = desc;

        mEventClickOk = onClickOk;
        mEventClickCancel = onClickCancel;
    }

	public void Set(string title, string desc, string inputHolderName, Action<string> onClickOk, Action onClickCancel, int limit)
	{
		TextInputHolder.text = inputHolderName;

		Set(title, desc, onClickOk, onClickCancel, limit);
	}

	public void OnClickOk()
    {
        mEventClickOk?.Invoke(InputName.text);
        OnClickClose();
    }

    public void OnClickCancel()
    {
        mEventClickCancel?.Invoke();
        OnClickClose();
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close<UIMessagePopupInput>();
    }
}