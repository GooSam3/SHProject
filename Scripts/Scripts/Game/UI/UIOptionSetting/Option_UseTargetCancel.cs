using UnityEngine;

public class Option_UseTargetCancel : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bUseTargetCancel)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetUseTargetCancel(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bUseTargetCancel)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 타겟 캔슬 버튼 사용 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_TargetCancel, _isOn);
        }
    }
}
