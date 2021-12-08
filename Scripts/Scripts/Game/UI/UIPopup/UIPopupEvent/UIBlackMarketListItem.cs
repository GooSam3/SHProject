using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIBlackMarketViewHolder : ZGridAdapterHolderBase<SpecialShop_Table>
{
	private UIBlackMarketListItem listItem;
	private Action<SpecialShop_Table> onClick;
	public override void SetSlot(SpecialShop_Table data)
	{
		listItem.SetSlot(data);
	}

	public override void CollectViews()
	{
		base.CollectViews();

		listItem = root.GetComponent<UIBlackMarketListItem>();
	}

	public void SetAction(Action<SpecialShop_Table> _onClick)
	{
		onClick = _onClick;
		listItem.SetAction(onClick);
	}
}

public class UIBlackMarketListItem : MonoBehaviour
{
	[Serializable]
	private class BlackMarketPrice
	{
		[SerializeField] private GameObject obj;
		[SerializeField] private Image icon;
		[SerializeField] private Text txt;

		public void SetPrice(bool state, string iconText, ulong price)
		{
			obj.SetActive(state);

			if (state == false)
				return;

			icon.sprite = UICommon.GetSprite(iconText);
			txt.text = price.ToString("N0");
		}
	}

	[SerializeField] private GameObject objDiscountTag;
	[SerializeField] private Text txtDiscountTag;

	[SerializeField] private Text buyLimit;
	[SerializeField] private GameObject objLimitLock;

	[SerializeField] private Image imgItem;
	[SerializeField] private Text txtItem;

	[SerializeField] private BlackMarketPrice priceNormal;

	[SerializeField] private BlackMarketPrice priceOrigin;
	[SerializeField] private BlackMarketPrice priceDiscount;

	private SpecialShop_Table data;
	private Action<SpecialShop_Table> onClickSlot;

	public void SetSlot(SpecialShop_Table _data)
	{
		data = _data;

		bool isSale = data.StateType == E_StateType.Sale;

		// 세일태그
		objDiscountTag.SetActive(isSale);
		if (isSale)
		{
			var discountValue = Mathf.RoundToInt((((float)data.BuyItemCount / (float)data.OriganalBuyCount) * 100f));
			txtDiscountTag.text = DBLocale.GetText("Event_BlackMarket_Discount", discountValue);
		}

		// 구매제한
		string strLimitCount = string.Empty;
		if (data.BuyLimitCount > 0)
		{
			var myLimit = Me.CurCharData.GetBuyLimitInfo(data.SpecialShopID)?.BuyCnt ?? 0;
			strLimitCount = DBLocale.GetText("Event_BlackMarket_BuyLimit", UICommon.GetProgressText((int)myLimit, (int)data.BuyLimitCount));
			objLimitLock.SetActive(myLimit == data.BuyLimitCount);
		}
		buyLimit.text = strLimitCount;


		if (DBItem.GetItem(data.GoodsItemID, out var itemTable) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"아이템없음~~~~~ spTid : {data.SpecialShopID}, item tid : {data.GoodsItemID}");
			objLimitLock.SetActive(true);
			return;
		}
		
		// 아이템 정보
		imgItem.sprite = UICommon.GetSprite(itemTable.IconID);
		txtItem.text = UICommon.GetItemText(itemTable);

		if (DBItem.GetItem(data.BuyItemID, out var buyItem) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"아이템없음~~~~~ spTid : {data.SpecialShopID}, item tid : {data.BuyItemID}");
			objLimitLock.SetActive(true);
			return;
		}

		// 가격 정보
		priceNormal.SetPrice(!isSale, buyItem.IconID, data.BuyItemCount);
		priceOrigin.SetPrice(isSale, buyItem.IconID, data.OriganalBuyCount);
		priceDiscount.SetPrice(isSale, buyItem.IconID, data.BuyItemCount);
	}

	public void SetAction(Action<SpecialShop_Table> onClick)
	{
		onClickSlot = onClick;
	}

	public void OnClickSlot()
	{
		onClickSlot?.Invoke(data);
	}
}
