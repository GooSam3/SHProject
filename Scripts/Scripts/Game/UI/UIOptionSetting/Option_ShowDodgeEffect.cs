using UnityEngine;

public class Option_ShowDodgeEffect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bShowDodgeEffect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void ShowDodgeEffect(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bShowDodgeEffect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 회피 이펙트 표시 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_DodgeEffect, _isOn);
        }
    }
}
