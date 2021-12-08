using UnityEngine;
using GameDB;
using ZDefine;
using UnityEngine.Events;

public class UIPopupStageMove : ZUIFrameBase
{
	public enum E_ChannelType
	{
		Normal,
		Chaos,	
	}

	[SerializeField] ZText			MoveCost;
	[SerializeField] ZText			Retained;
	[SerializeField] ZImage		IconCost;
	[SerializeField] ZImage		IconRetained;
	[SerializeField] GameObject	IconChaos;
	[SerializeField] ZText			Destinaiton;

	private Portal_Table		mPortalTable = null;
	private E_ChannelType		mChannelType = E_ChannelType.Normal;
	private UnityAction<bool> mFinishPopup = null;
	//---------------------------------------------------------
	public void DoUIStageMove(E_ChannelType _costType, uint _portalID, UnityAction<bool> _finishPopup = null)
	{
		mChannelType = _costType;
		mPortalTable = DBPortal.Get(_portalID);
		mFinishPopup = _finishPopup;

		if (mPortalTable == null)
		{
			Close();
			return;
		}

		Stage_Table stageTable = DBStage.Get(mPortalTable.StageID);
		if (stageTable != null)
		{
			Destinaiton.text = stageTable.StageTextID;
		}

		if (_costType == E_ChannelType.Normal) 
		{
			ArrangeCostIcon(mPortalTable.UseItemID, mPortalTable.UseItemCount);
			IconChaos.gameObject.SetActive(false);
		}
		else if (_costType == E_ChannelType.Chaos)
		{
			ArrangeCostIcon(mPortalTable.ChaosChannelUseItemID, mPortalTable.ChaosChannelUseItemCnt);
			IconChaos.gameObject.SetActive(true);
		}
	}

	//---------------------------------------------------------
	private void ArrangeCostIcon(uint _itemID, uint _itemCount)
	{
		if (_itemID == 0)
		{
			_itemID = DBConfig.Gold_ID;
		}

		MoveCost.text = _itemCount == 0 ? "Warp_Cost_Null" : _itemCount.ToString();
		Retained.text = ZNet.Data.Me.GetCurrency(_itemID).ToString();
		Item_Table itemTable = DBItem.GetItem(_itemID);
		IconCost.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
		IconRetained.sprite = IconCost.sprite;
	}

	//---------------------------------------------------------
	public void HandleStageMoveConfirm()
	{
		Close();
		mFinishPopup?.Invoke(true);
		ZItem item = null;
		if (mChannelType == E_ChannelType.Normal)
		{
			item = ZNet.Data.Me.CurCharData.GetItem(mPortalTable.UseItemID);
			ZGameManager.Instance.TryEnterStage(mPortalTable.PortalID, false, item?.item_id ?? 0, item?.item_tid ?? 0);
		}
		else if (mChannelType == E_ChannelType.Chaos)
		{
			item = ZNet.Data.Me.CurCharData.GetItem(mPortalTable.ChaosChannelUseItemID);
			ZGameManager.Instance.TryEnterStage(mPortalTable.PortalID, true, item?.item_id ?? 0, item?.item_tid ?? 0);
		}		
	}

	public void HandleStageMoveCancle()
	{
		Close();
		mFinishPopup?.Invoke(false);
	}

}
 