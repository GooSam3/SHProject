using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZDefine;

public class UITradeSettlementAdapter : OSA<BaseParamsWithPrefab, UITradeSettlementViewsHolder>
{
	public enum E_SettlementType
	{
		Settlement = 0,
		Log = 1,
	}

	public SimpleDataHelper<ScrollTradeSettlementData> Data
	{
		get; private set;
	}

	protected override UITradeSettlementViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UITradeSettlementViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UITradeSettlementViewsHolder newOrRecycled)
	{
		ScrollTradeSettlementData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
				UpdateViewsHolder(GetItemViewsHolder(i));
		}
	}

	/// <summary>Adapter 초기 세팅 (최초 1회)</summary>
	public void Initialize()
	{
		if (Parameters.ItemPrefab == null)
		{
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITradeSettlementViewsHolder));
			Parameters.ItemPrefab = TradeHodler.GetComponent<RectTransform>();
			Parameters.ItemPrefab.SetParent(GetComponent<Transform>());
			Parameters.ItemPrefab.transform.localScale = Vector2.one;
			Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
			Parameters.ItemPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.ItemPrefab != null);
		}

		Init();
	}

	private void ClearData()
	{
		Data.List.Clear();
	}

	public void SetData(E_SettlementType _type, Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollTradeSettlementData>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직
		switch (_type)
		{
			case E_SettlementType.Settlement:
				ZWebManager.Instance.WebGame.GetExchangeSoldOutList(WebNet.E_ExchangeTransactionState.SoldOut, 0, (recvPacket, recvSoldList) => {
					callback(recvSoldList);
					if (UIManager.Instance.Find(out UIFrameTrade _trade))
						_trade.RefreshTextData(E_TradeMenu.Settlement);
				});
				break;

			case E_SettlementType.Log:
				ZWebManager.Instance.WebGame.GetExchangeSoldOutList(WebNet.E_ExchangeTransactionState.TakeMoney, 0, (recvPacket, recvSoldList) => {
					callback(recvSoldList);
					if (UIManager.Instance.Find(out UIFrameTrade _trade))
						_trade.RefreshTextData(E_TradeMenu.SettlementLog);
				});
				break;

				void callback(ResExchangeSoldOutList recvSoldList)
				{
					for (int i = 0; i < recvSoldList.SoldOutsLength; i++)
					{
						var soldItem = Data.List.Find(item => item.Data.ExchangeID == recvSoldList.SoldOuts(i).Value.ExchangeId);

						if (soldItem == null)
							Data.List.Add(new ScrollTradeSettlementData() { Data = new SoldOutItemData(recvSoldList.SoldOuts(i).Value) });
						else
							soldItem.Reset(new ScrollTradeSettlementData() { Data = new SoldOutItemData(recvSoldList.SoldOuts(i).Value) });
					}

					Data.NotifyListChangedExternally();

					_callback?.Invoke();
				}
		}
		#endregion
	}
}

public class ScrollTradeSettlementData
{
	public SoldOutItemData Data;
	public ZItem Item;

	public void Reset(ScrollTradeSettlementData _data)
	{
		this.Data = _data.Data;
	}
}

public class UITradeSettlementViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image ItemIcon;
	private Image ItemGradeBoard;
	private Text ItemGrade;
	private Text ItemName;
	private Text ItemNum;
	private Text ItemInfo;
	private Text ItemSellTime;
	private Text ItemSellCost;
	private Text ItemSettleCost;
	private Image ItemSettleIcon;
	#endregion

	#region OSA System Variable
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Share_Parts/Item_Icon", out ItemIcon);
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Share_Parts/Grade_Board", out ItemGradeBoard);
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Inven_Parts/Grade/Txt_Grade", out ItemGrade);
		root.GetComponentAtPath("Item/Txt_Box/Txt_ItemName", out ItemName);
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Inven_Parts/Num/Txt_Num", out ItemNum);
		root.GetComponentAtPath("Item/Txt_Box/Txt_ItemOption", out ItemInfo);
		root.GetComponentAtPath("Sell_Time/Txt_SellTime", out ItemSellTime);
		root.GetComponentAtPath("Sell_Cost/Txt_Cost", out ItemSellCost);
		root.GetComponentAtPath("Settle_Cost/Txt_Cost", out ItemSettleCost);
		root.GetComponentAtPath("Settle_Cost/Icon_Wealth", out ItemSettleIcon);
	}

	public void UpdateViews(ScrollTradeSettlementData _model)
	{
		var itemData = DBItem.GetItem(_model.Data.ItemTId);

		if (itemData == null)
			return;

		ItemIcon.sprite = UICommon.GetItemIconSprite(itemData.ItemID);
		ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(itemData.ItemID);
		ItemGrade.text = itemData.ItemUseType == GameDB.E_ItemUseType.Equip && itemData.Step > 0 ? "+" + itemData.Step.ToString() : string.Empty;
		ItemName.text = DBLocale.GetText(itemData.ItemTextID);
		ItemNum.text = itemData.ItemUseType != GameDB.E_ItemUseType.Equip ? _model.Data.ItemCnt.ToString() : string.Empty;
		//서버에서 제련 옵션 정보 주지 않는 것 같으니 확인 필요 (일단 string.empty로 작업)
		ItemInfo.text = string.Empty;
		ItemSellCost.text = (_model.Data.ItemPrice * _model.Data.ItemCnt).ToString();
		ItemSettleCost.text = (_model.Data.ItemTotalPrice - _model.Data.ItemTotalPriceTex).ToString();
		ItemSettleIcon.sprite = UICommon.GetItemIconSprite(DBConfig.Diamond_ID);
		ItemSellTime.text = TimeHelper.ParseTimeStamp((long)_model.Data.SellDt).ToString();
	}
}