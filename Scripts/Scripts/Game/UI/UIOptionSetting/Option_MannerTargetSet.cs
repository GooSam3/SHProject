using UnityEngine;

public class Option_MannerTargetSet : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bManner_Search)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void SetMannerMode(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bManner_Search)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 매너타겟 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_MannerSearch, _isOn);
        }
    }
}
