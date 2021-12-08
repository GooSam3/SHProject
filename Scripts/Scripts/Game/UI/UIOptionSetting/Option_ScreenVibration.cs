using UnityEngine;

public class Option_ScreenVibration : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bVibration)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }
    public void SetScreenVibration(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bVibration)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 화면 흔들림 설정 " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Vibration, _isOn);
        }
    }
}
