using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

// 1f , .25f

public class UIGatheringDataHolder : ZAdapterHolderBase<OSA_EventGatheringData>
{
	private UIGatheringListItem listItem;
	private Action<OSA_EventGatheringData> onClick;

	public override void SetSlot(OSA_EventGatheringData data)
	{
		listItem.SetSlot(data);
	}

	public override void CollectViews()
	{
		base.CollectViews();

		listItem = root.GetComponent<UIGatheringListItem>();
	}

	public void SetAction(Action<OSA_EventGatheringData> _onclick)
	{
		onClick = _onclick;
		listItem.SetAction(onClick);
	}

}

public class UIGatheringListItem : MonoBehaviour
{
	private const float ALPHA_OFF = .25f;
	private const float ALPHA_ON = 1f;

	[SerializeField] private CanvasGroup canvasGroup;       // 총 보상 여부에따른 알파값 조정

	[SerializeField] private UIItemSlot rewardItemSlot;     // 보상 아이템
	[SerializeField] private Text txtRewardItem;            // 보상 아이템 이름

	[SerializeField] private Text progressRewardLimit;      // 교환 가능 횟수

	[SerializeField] private Image imgMatIcon;              // 재료 아이콘
	[SerializeField] private Text txtMatNeedCount;          // 필요 재료 수
	[SerializeField] private ZButton btnExchange;           // 교환 버튼

	[SerializeField] private ZButton btnExchangePrev;       // 교환 갯수 설정 -
	[SerializeField] private ZButton btnExchangeNext;       // 교환 갯수 설정 +
	[SerializeField] private Text txtExcnahgeCount;         // 교환 갯수

	private Action<OSA_EventGatheringData> onClickExchange; // 교환 클릭시 액션

	private OSA_EventGatheringData data;                    // 현재 데이터

	private int exchangeMin;                                // 현재 교환 갯수( NetData에 저장된 )
	private int exchangeMax;                                // 교환 가능 갯수

	private int exchangeValue;                              // 교환 갯수 ( 보여주기용 )

	public void SetSlot(OSA_EventGatheringData _data)
	{
		data = _data;

		if (DBItem.GetItem(data.table.GoodsItemID, out var itemTable) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"보상 아이템 음슴 shoptid : {data.table.SpecialShopID}");
			return;
		}

		if (DBItem.GetItem(data.table.BuyItemID, out var matTable) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"재료 아이템 음슴 shoptid : {data.table.SpecialShopID}");
			return;
		}

		rewardItemSlot.SetItem(data.table.GoodsItemID, data.table.GoodsCount);
		rewardItemSlot.SetPostSetting(UIItemSlot.E_Item_PostSetting.LockOff | UIItemSlot.E_Item_PostSetting.ShadowOff);

		txtRewardItem.text = UICommon.GetItemText(itemTable);

		imgMatIcon.sprite = UICommon.GetSprite(matTable.IconID);
		txtMatNeedCount.text = data.table.BuyItemCount.ToString();

		exchangeMin = (int)(Me.CurCharData.GetBuyLimitInfo(data.table.SpecialShopID)?.BuyCnt ?? 0);
		exchangeMax = (int)data.table.BuyLimitCount;
		exchangeValue = 0;

		progressRewardLimit.text = UICommon.GetProgressText(exchangeMin, exchangeMax, false);

		canvasGroup.alpha = (exchangeMin == exchangeMax) ? ALPHA_OFF : ALPHA_ON;

		RefreshExchangeUI();
	}

	public void SetAction(Action<OSA_EventGatheringData> _onclick)
	{
		onClickExchange = _onclick;
	}

	public void OnClickExchangeNum(bool isNext)
	{
		var tempValue = exchangeValue + (isNext ? 1 : -1);

		if (tempValue > exchangeMax || tempValue < 0)
			return;

		exchangeValue = tempValue;

		RefreshExchangeUI();

	}

	public void OnClickConfirm()
	{
		onClickExchange?.Invoke(data);
	}

	public void RefreshExchangeUI()
	{
		btnExchangePrev.interactable = exchangeValue > 0;
		btnExchangeNext.interactable = exchangeValue < (exchangeMax - exchangeMin);

		txtExcnahgeCount.text = exchangeValue.ToString();

		data.selectValue = exchangeValue;

		var mul = exchangeValue == 0 ? 1 : exchangeValue;

		ulong resultExchangeValue = (ulong)(mul * data.table.BuyItemCount);
		bool isExchangable = resultExchangeValue <= (Me.CurCharData.GetItem(data.table.BuyItemID)?.cnt ?? 0);

		btnExchange.interactable = isExchangable;

		txtMatNeedCount.text = resultExchangeValue.ToString();
		txtMatNeedCount.color = isExchangable ? Color.white : Color.red;

	}
}
