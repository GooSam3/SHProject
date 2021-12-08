using UnityEngine;

public class Option_AutoScreenSaverModeOff : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;
    [SerializeField] private CanvasGroup AlphaCanvasGroup;

    public override void OnEnable()
    {
        base.OnEnable();

        ZGameOption.Instance.OnOptionChanged += OptionChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        ZGameOption.Instance.OnOptionChanged -= OptionChanged;
    }

    /// <summary>
    /// 자동 절전 모드 시간을 설정하지 않았으면 피격 시 절전 모드 자동 해제 기능 끄기위해 이벤트 연결
    /// </summary>
    void OptionChanged(ZGameOption.OptionKey optionKey)
    {
        if(optionKey == ZGameOption.OptionKey.Option_AutoScreenSaverTime)
        {
            LoadOption();
        }
    }

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.Auto_Screen_Saver_Time <= 0)
        {
            AlphaCanvasGroup.enabled = true;
            AlphaCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            AlphaCanvasGroup.enabled = false;
            AlphaCanvasGroup.blocksRaycasts = true;
        }

        if (ZGameOption.Instance.bAuto_Wakeup_ScreenSaver)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetAutoScreenSaverOff(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bAuto_Wakeup_ScreenSaver)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 절전 모드 해제 설정 : "+ _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_WakeUp_ScreenSaver, _isOn);
        }
    }
}
