using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupConsole : UIPopupBase
{
    #region UI Variable
    [SerializeField] private Text TitleName;
    [SerializeField] private Text Content;
    [SerializeField] private Button[] Button = new Button[ZUIConstant.CONSOLE_POPUP_BUTTON_COUNT];
    [SerializeField] private Text[] ButtonContent = new Text[ZUIConstant.CONSOLE_POPUP_BUTTON_COUNT];
    #endregion

    #region System Variable
    private Action[] mCallback = new Action[ZUIConstant.CONSOLE_POPUP_BUTTON_COUNT];
    #endregion

    public void Open(string _title, string _content, string[] _btnTxt, Action[] _callBack)
    {
        Initialize();

        transform.SetParent(UIManager.Instance.gameObject.transform);
        transform.localPosition = Vector2.zero;
        transform.localScale = Vector2.one;

        Set(Button, ButtonContent, mCallback, _btnTxt, _callBack);

        TitleName.text = _title;
        Content.text = _content;
    }

    protected override void Active(bool _active)
    {
        if (!_active)
            Destroy(gameObject);
    }

    public void Close(bool bDestroy = false)
    {
		if (bDestroy)
            Destroy(this.gameObject); 
		else
            Active(false);
    }

    private void Initialize()
    {
        TitleName.text = string.Empty;
        Content.text = string.Empty;

        for (int i = 0; i < ButtonContent.Length; i++)
        {
            ButtonContent[i].text = string.Empty;
            Button[i].gameObject.SetActive(false);
        }
    }

    public void InputButton(int _buttonCnt)
    {
        Close(true);

        mCallback[_buttonCnt]?.Invoke();
    }
}