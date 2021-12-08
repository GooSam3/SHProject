using UnityEngine;

public class Option_AlramFriendConnect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bAlram_Friend_Connect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetFriendConnectAlram(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bAlram_Friend_Connect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 친구 접속 알림 설정 " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AlramFriendConnect, _isOn);
        }
    }
}
