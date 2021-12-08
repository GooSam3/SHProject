using UnityEngine;

public class Option_AlramBeAttacked : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bAlramBeAttacked_PC)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetBeAttackedAlram(bool _isOn)
    {
        if(_isOn != ZGameOption.Instance.bAlramBeAttacked_PC)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 피격 시 알림 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Alram_BeAttacked, _isOn);
        }
    }
}
