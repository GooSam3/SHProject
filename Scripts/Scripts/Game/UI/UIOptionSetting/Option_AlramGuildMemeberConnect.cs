using UnityEngine;

public class Option_AlramGuildMemeberConnect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bAlram_Guild_Member_Connect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetGuildMemberConnectAlram(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bAlram_Guild_Member_Connect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 길드원 접속 알림 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AlramGuildMemeberConnect, _isOn);
        }
    }
}
