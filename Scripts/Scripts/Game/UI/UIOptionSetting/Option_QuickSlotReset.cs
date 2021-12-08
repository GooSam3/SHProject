public class Option_QuickSlotReset : OptionSetting
{
    public void ClickReset()
    {
        ZNet.Data.Me.CurCharData.RemoveAllQuickSlotData();

        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.QuickSlot_Set1, ZNet.Data.Me.CurCharData.GetQuickSlotValue(0), (recvPacket, recvMsgPacket) =>
        {
            ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.QuickSlot_Set2, ZNet.Data.Me.CurCharData.GetQuickSlotValue(1), (recvPacket2, recvMsgPacket2) =>
            {
                if(UIManager.Instance.Find(out UISubHUDQuickSlot _quickSlot))
                    _quickSlot.ReSetAllSlot();
            });
        });
    }
}
