using UnityEngine;

public class Option_AllowPartyJoin : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bAllowPartyJoin)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetAllowPartyJoin(bool _isOn)
    {
        if(_isOn != ZGameOption.Instance.bAllowPartyJoin)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 파티 참여 신청 수신 설정 " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AllowPartyJoin, _isOn);
        }
    }
}
