using UnityEngine;

public class Option_UseVirtualPad : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if(ZGameOption.Instance.bUseVirtualPad)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetUseVirtualPad(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bUseVirtualPad)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 버추얼 패드 사용 설정 " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Use_VirtualPad, _isOn);
        }
    }
}
