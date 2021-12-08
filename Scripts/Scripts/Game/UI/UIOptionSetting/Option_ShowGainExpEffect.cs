using UnityEngine;

public class Option_ShowGainExpEffect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bShowExpGainEffect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void ShowGainExpEffect(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bShowExpGainEffect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 경험치 획득 이펙트 표시 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ExpGainEffect, _isOn);
        }
    }
}
