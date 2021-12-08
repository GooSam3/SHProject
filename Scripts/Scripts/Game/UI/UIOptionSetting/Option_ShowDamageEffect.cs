using UnityEngine;

public class Option_ShowDamageEffect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bShowDamageEffect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void ShowDamageEffect(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bShowDamageEffect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 데미지 이펙트 표시 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_DamageEffect, _isOn);
        }
    }
}
