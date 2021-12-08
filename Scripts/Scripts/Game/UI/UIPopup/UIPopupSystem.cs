using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupSystem : UIPopupBase
{
    #region UI Variable
    [SerializeField] private Text TitleName;
    [SerializeField] private Text Content;
    [SerializeField] private Button[] Button = new Button[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    [SerializeField] private Image[] ButtonBG = new Image[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    [SerializeField] private Text[] ButtonContent = new Text[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    #endregion

    #region System Variable
    private Action[] mCallback = new Action[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];
    #endregion

    private string[] ButtonBGDefault = new string[ZUIConstant.SYSTEM_POPUP_BUTTON_COUNT];

    public void Open(string _title, string _content, string[] _btnTxt, Action[] _callBack)
    {
        Initialize();

        //transform.SetParent(UIManager.Instance.gameObject.transform);
        transform.localPosition = Vector2.zero;
        transform.localScale = Vector2.one;

        SetDefaultBG();

        Set(Button, ButtonContent, mCallback, _btnTxt, _callBack);

        TitleName.text = _title;
        Content.text = _content.Replace("\\n", "\n");
    }

    protected override void OnUnityAwake()
    {
        for (int i = 0; i < ButtonContent.Length; i++)
        {
            ButtonBGDefault[i] = ButtonBG[i].sprite.name;
        }
    }

    void SetDefaultBG()
    {
        for (int i = 0; i < ButtonContent.Length; i++)
        {
            ButtonBG[i].sprite = ZManagerUIPreset.Instance.GetSprite(ButtonBGDefault[i]);
        }
    }

    public void SetButtonBG(int ButtonIndex,string BGName)
    {
        if(ButtonIndex < ButtonContent.Length)
            ButtonBG[ButtonIndex].sprite = ZManagerUIPreset.Instance.GetSprite(BGName);
    }

    protected override void Active(bool _active)
    {
        if (!_active)
            UIManager.Instance.Close<UIPopupSystem>();
    }

    public void Close()
    {
        UIManager.Instance.Close<UIPopupSystem>();
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
        mCallback[_buttonCnt]?.Invoke();
    }
}