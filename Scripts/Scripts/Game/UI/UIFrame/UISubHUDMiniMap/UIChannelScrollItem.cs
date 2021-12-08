using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIChannelScrollItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] GameObject	ChannelChaos;
	[SerializeField] GameObject	ChannelNormal;
	[SerializeField] ZText			NameChaos;
	[SerializeField] ZText			NameNormal;
	[SerializeField] ZText			ChaosOwner;
	[SerializeField] ZText			CostChaos;
	[SerializeField] ZButton		ButtonChaos;
	[SerializeField] ZButton		ButtonNormal;

	[SerializeField] ZImage		SelectArrow;
	[SerializeField] ZImage	    IconChaos;
	[SerializeField] GameObject    BusyMark;

	private uint mChannelNum = 0;
	private bool mChaosChannel = false;
	private UIMinimapChannelPopup mChannelProcedure;
	//---------------------------------------------------------------------
	public void DoChannelItemNormal(uint _channelNum, bool _selected, bool _busy, UIMinimapChannelPopup _channelProcedure)
	{
		mChannelProcedure = _channelProcedure;
		mChannelNum = _channelNum;
		mChaosChannel = false;

		ChannelChaos.SetActive(false);
		ChannelNormal.SetActive(true);

		NameNormal.text = string.Format("채널  {0}", _channelNum.ToString());
		
		if (_selected)
		{
			SelectArrow.gameObject.SetActive(true);
		}
		else
		{
			SelectArrow.gameObject.SetActive(false);
		}

		if (_busy)
		{
			BusyMark.gameObject.SetActive(true);
			ButtonNormal.interactable = false;
		}
		else
		{
			BusyMark.gameObject.SetActive(false);
			ButtonNormal.interactable = true;
		}
	}

	public void DoChannelItemChaos(uint _channelNum, string _ownerGuildName, GameDB.Portal_Table _portalTable, bool _selected, bool _busy, UIMinimapChannelPopup _channelProcedure)
	{
		mChannelProcedure = _channelProcedure;
		mChannelNum = _channelNum;
		mChaosChannel = true;

		ChannelChaos.SetActive(true);
		ChannelNormal.SetActive(false);

		ChaosOwner.text = _ownerGuildName;
		CostChaos.text = _portalTable.ChaosChannelUseItemCnt.ToString();

		NameChaos.text = "CHAOS_Channel_Name";

		GameDB.Item_Table itemTable = DBItem.GetItem(_portalTable.ChaosChannelUseItemID);
		ZManagerUIPreset.Instance.SetSprite(IconChaos, itemTable.IconID);

		if (_selected)
		{
			SelectArrow.gameObject.SetActive(true);
		}
		else
		{
			SelectArrow.gameObject.SetActive(false);
		}

		if (_busy)
		{
			BusyMark.gameObject.SetActive(true);
		}
		else
		{
			BusyMark.gameObject.SetActive(false);
		}

	
	}

	//-------------------------------------------------------------------------
	public void HandleChannelSelect()
	{
		mChannelProcedure.DoMinimapChannelSelect(mChannelNum, mChaosChannel);
	}

}
