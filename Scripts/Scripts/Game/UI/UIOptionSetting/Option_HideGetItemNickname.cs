using UnityEngine;

public class Option_HideGetItemNickname : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    bool bHideName = true;

    public override void LoadOption()
    {
        base.LoadOption();

        long accountOption = ZNet.Data.Me.AccountOptionBit;
        int value = 1 << (int)WebNet.E_AccountOptionType.ItemDropCharacterNickNone;

        bHideName = (accountOption & value) != 0;

        if (bHideName)
            OnButton.SelectToggle();
        else
            OffButton.SelectToggle();

        /*
        if (ZGameOption.Instance.bHideGetitemNickName)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }
        */

    }

    public void SetHideNickNameGainItemMessage(bool _isOn)
    {
        if ((_isOn == true && !bHideName ) || (_isOn == false && bHideName))
        {
            ZWebManager.Instance.WebGame.REQ_SetAccountOption(WebNet.E_AccountOptionType.ItemDropCharacterNickNone, _isOn, (recvPacket, recvbMsgPacket) =>
            {
                ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 아이템 획득메시지 닉네임 숨김 : " + _isOn);
                ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_HideGetitemNickName, _isOn);
                bHideName = _isOn;
            });
            
        }
    }
}
