using UnityEngine;

public class Option_AlramEnemyConnect : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bAlram_Enemy_Connect)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetEnemyConnectAlram(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bAlram_Enemy_Connect)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 경계 접속 알림 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AlramEnemyConnect, _isOn);
        }
    }
}
