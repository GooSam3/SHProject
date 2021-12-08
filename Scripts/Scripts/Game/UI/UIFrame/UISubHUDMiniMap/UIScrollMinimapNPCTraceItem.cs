using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;
using UnityEngine.Events;
public class UIScrollMinimapNPCTraceItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] ZImage	NPCIcon;
	[SerializeField] ZText		NPCName;

	private uint		mNPCTableID = 0;
	private UnityAction<uint> mEventSelectItem = null;
	//-----------------------------------------------------
	public void DoMinimapNpcTraceInfo(NPC_Table _npcTalble, UnityAction<uint> _eventSelectItem)
	{
		mNPCTableID = _npcTalble.NPCID;
		mEventSelectItem = _eventSelectItem;
		ZManagerUIPreset.Instance.SetSprite(NPCIcon, _npcTalble.Icon);
		NPCName.text = _npcTalble.NPCTextID;
	}
	//-----------------------------------------------------
	public void HandleNPCTraceSelect()
	{
		mEventSelectItem?.Invoke(mNPCTableID);
	}


}
