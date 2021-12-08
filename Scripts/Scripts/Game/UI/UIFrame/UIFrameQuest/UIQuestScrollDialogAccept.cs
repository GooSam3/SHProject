using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQuestScrollDialogAccept : CUGUIScrollRectBase
{
    private List<UIQuestScrollBase.SQuestInfo> m_listQuestInfo = null;
	//-------------------------------------------------------------------------
	public void DoDialogAccept(List<UIQuestScrollBase.SQuestInfo> _listQuestInfo, UIQuestDialogNPCAccept _dialogOwner)
	{
		ProtUIScrollSlotItemReturnAll();
        m_listQuestInfo = _listQuestInfo;

		for (int i = m_listQuestInfo.Count - 1 ; i >= 0 ; i--)
		{
			UIQuestScrollDialogAcceptItem item = ProtUIScrollSlotItemRequest() as UIQuestScrollDialogAcceptItem;
			item.DoDialogAcceptItem(m_listQuestInfo[i], _dialogOwner);
		}
	}
}
