using UnityEngine;

public class Option_ShowDefaultCharacterInfo : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if(ZGameOption.Instance.bShowDefaultCharacterText)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void ShowDefaultCharacterInfo(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bShowDefaultCharacterText)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 캐릭터 정보 표시 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ShowDefaultCharacterText, _isOn);
        }
    }
}
