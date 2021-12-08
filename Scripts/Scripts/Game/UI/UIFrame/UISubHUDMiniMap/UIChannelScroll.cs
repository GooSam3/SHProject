using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIChannelScroll : CUGUIScrollRectBase
{

    //-------------------------------------------------------------
    public void DoChannelScrollAdd(ZGameModeManager.SChannelData _channelData, uint _currentChannel, GameDB.Portal_Table _portalTable, UIMinimapChannelPopup _channelProcedure)
	{
        UIChannelScrollItem item = ProtUIScrollSlotItemRequest() as UIChannelScrollItem;
        item.DoChannelItemNormal(_channelData.ChannelID, _channelData.ChannelID == _currentChannel, _channelData.UserCur >= _channelData.UserMax, _channelProcedure);
	}
     
    public void DoChannelScrollReset()
    {
        ProtUIScrollSlotItemReturnAll();
    }
    //-------------------------------------------------------------

}
