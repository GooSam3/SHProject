using UnityEngine;

public class Option_ShowGainGoldEffect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bShowGoldGainEffect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void ShowGainGoldEffect(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bShowGoldGainEffect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 골드 획득 이펙트 표시 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_GoldGainEffect, _isOn);
        }
    }
}
