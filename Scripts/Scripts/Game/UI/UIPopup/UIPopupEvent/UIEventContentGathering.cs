using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEventContentGathering : UIEventContentBase
{
	[SerializeField] private Sprite sprUniqueRewardOn;
	[SerializeField] private Sprite sprUniqueRewardOff;

	[SerializeField] private Text txtEventTimeRange;
	[SerializeField] private Text txtExchangeTimeRange;

	[SerializeField] private UIGatheringListAdapter osaGathering;

	[SerializeField] private UIItemSlot uniqueReward;
	[SerializeField] private ZButton btnUniqueReward;
	[SerializeField] private Text txtUniqueReward;
	[SerializeField] private GameObject objUniqueRewardCheck;

	[SerializeField] private Text txtNeedMatCount;
	[SerializeField] private Image imgRewardMatIcon;

	[SerializeField] private Image imgOwnMatIcon;
	[SerializeField] private Text txtOwnMatCount;

	private List<OSA_EventGatheringData> listGatheringData = new List<OSA_EventGatheringData>();
	private SpecialShop_Table mainReward;
	// 재료
	protected override bool SetContent(IngameEventInfoConvert _eventData)
	{
		listGatheringData.Clear();

		// 이벤트 정보 설정
		var list = DBSpecialShop.GetShopList(E_SpecialShopType.CollectEvent);
		mainReward = null;

		foreach (var iter in list)
		{
			if (iter.ViewType != E_ViewType.View)
				continue;
			if (iter.UnusedType != E_UnusedType.Use)
				continue;
			if (iter.GroupID != _eventData.groupId)
				continue;

			if (iter.SizeType == E_SizeType.Big && iter.PositionNumber == 1)
			{
				mainReward = iter;
			}
			else
			{
				listGatheringData.Add(new OSA_EventGatheringData(iter));
			}
		}

		//보상, 재료 설정
		uniqueReward.SetItem(mainReward.GoodsItemID, mainReward.GoodsCount);
		uniqueReward.SetPostSetting(UIItemSlot.E_Item_PostSetting.ShadowOff | UIItemSlot.E_Item_PostSetting.LockOff);

		txtNeedMatCount.text = mainReward.BuyItemCount.ToString();

		RefreshCommonUI();

		if (DBItem.GetItem(mainReward.BuyItemID, out var itemTable))
		{
			imgRewardMatIcon.sprite = UICommon.GetSprite(itemTable.IconID);
			imgOwnMatIcon.sprite = UICommon.GetSprite(itemTable.IconID);
		}
		else
		{
			imgRewardMatIcon.sprite = null;
			imgOwnMatIcon.sprite = null;
		}

		if (DBItem.GetItem(mainReward.GoodsItemID, out var rewardTable))
		{
			txtUniqueReward.text = UICommon.GetItemText(rewardTable);
		}
		else
		{
			txtUniqueReward.text = string.Empty;
		}

		//osa
		if (osaGathering.IsInitialized == false)
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIGatheringListItem), delegate
			{
				osaGathering.Initialize(OnClickConfirm);
				osaGathering.ResetListData(listGatheringData);

				IsLoadSuccess = true;
			});
			return false;
		}
		else
		{
			osaGathering.ResetListData(listGatheringData);
		}

		return true;
	}

	private void RefreshCommonUI()
	{
		bool hasUniqueReward = (Me.CurCharData.GetBuyLimitInfo(mainReward.SpecialShopID)?.BuyCnt ?? 0) >= mainReward.BuyLimitCount;

		objUniqueRewardCheck.SetActive(hasUniqueReward);
		txtOwnMatCount.text =(Me.CurCharData.GetItem(mainReward.BuyItemID)?.cnt??0).ToString();
		btnUniqueReward.interactable = !hasUniqueReward;
	}


	private void OnClickConfirm(OSA_EventGatheringData data)
	{
		// 한번 더 체크

		var myCnt = Me.CurCharData.GetBuyLimitInfo(data.table.SpecialShopID)?.BuyCnt ?? 0;
		var resultLimit = myCnt + data.selectValue;

		if (data.selectValue == 0)
		{
			UIMessagePopup.ShowPopupOk("Message_Error_SelectValue_Empty");
			return;
		}

		if (resultLimit > data.table.BuyLimitCount)
		{
			UIMessagePopup.ShowPopupOk("NO_MORE_BUY_ITEM");
			ZLog.Log(ZLogChannel.Event, $"구매제한갯수를 넘겼다~~, shopId : {data.table.SpecialShopID}, My BuyCnt : {myCnt},select : {data.selectValue}, result BuyCnt: {resultLimit}, table Limit : {data.table.BuyLimitCount}");
			return;
		}

		if (ConditionHelper.CheckCompareCost(data.table.BuyItemID, data.table.BuyItemCount) == false)
			return;

		var itemData = new DBSpecialShop.ItemBuyInfo(data.table.SpecialShopID,
										  (uint)data.selectValue,
										  Me.CurCharData.GetItem(data.table.BuyItemID).item_id,
										  data.table.BuyItemID,
										  data.table.GoodsItemID);


		ZWebManager.Instance.WebGame.REQ_SpecialShopBuy(new List<DBSpecialShop.ItemBuyInfo>() { itemData }, (recvPacket, recvMsgPacket) =>
		 {
			 var listGain = new List<GainInfo>();

			 for (int i = 0; i < recvMsgPacket.ResultGetItemsLength; i++)
			 {
				 listGain.Add(new GainInfo(recvMsgPacket.ResultGetItems(i).Value));
			 }

			 if (listGain.Count > 0)
			 {
				 UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
				 {
					 frame.AddItem(listGain);
				 });
			 }

			 RefreshCommonUI();

			 data.selectValue = 0;
			 osaGathering.Refresh();
		 });
	}

	public void OnClickUniqueReward()
	{
		OnClickConfirm(new OSA_EventGatheringData(mainReward) { selectValue = 1 });
	}

	protected override void ReleaseContent()
	{
	}
}
