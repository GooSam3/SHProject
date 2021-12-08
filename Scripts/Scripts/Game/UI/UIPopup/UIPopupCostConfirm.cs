using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupCostConfirm : UIPopupBase
{
    #region UI Variable
    [SerializeField] private Text TitleName;
    [SerializeField] private Text Content;
    [SerializeField] private Text CostValue;
    [SerializeField] private ZImage CostIcon;
    [SerializeField] private Button[] Button = new Button[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    [SerializeField] private Text[] ButtonContent = new Text[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    #endregion

    #region System Variable
    private Action[] mCallback = new Action[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    #endregion

    public void Open(string _title, string _content, string _costValue, string _costIcon, string[] _btnTxt, Action[] _callBack)
    {
        Initialize();

       // transform.SetParent(UIManager.Instance.gameObject.transform);
        transform.localPosition = Vector2.zero;
        transform.localScale = Vector2.one;

        Set(Button, ButtonContent, mCallback, _btnTxt, _callBack);

        TitleName.text = _title;
        Content.text = _content.Replace("\\n", "\n");
        CostValue.text = _costValue;

        CostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(_costIcon);
    }

    protected override void Active(bool _active)
    {
        if (!_active)
            UIManager.Instance.Close<UIPopupCostConfirm>();
    }

    public void Close()
    {
        UIManager.Instance.Close<UIPopupCostConfirm>();
    }

    private void Initialize()
    {
        TitleName.text = string.Empty;
        Content.text = string.Empty;
        CostValue.text = string.Empty;

        for (int i = 0; i < ButtonContent.Length; i++)
        {
            ButtonContent[i].text = string.Empty;
            Button[i].gameObject.SetActive(false);
        }
    }

    public void InputButton(int _buttonCnt)
    {
        mCallback[_buttonCnt]?.Invoke();
    }
}
