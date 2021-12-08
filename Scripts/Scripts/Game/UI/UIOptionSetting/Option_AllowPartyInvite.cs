using UnityEngine;

public class Option_AllowPartyInvite : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bAllowPartyInvite)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetAllowPartyInvite(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bAllowPartyInvite)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 파티 초대 수신 설정 " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AllowPartyInvite, _isOn);
        }
    }
}
