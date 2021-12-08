using UnityEngine;

public class Option_ShowCharacterName : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void LoadOption()
    {
        base.LoadOption();

        if (ZGameOption.Instance.bShowCharacterName)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
    }

    public void ShowCharacterName(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bShowCharacterName)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 캐릭터 닉네임 표시 설정 : " + _isOn);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_ShowCharacterName, _isOn);
        }
    }
}
