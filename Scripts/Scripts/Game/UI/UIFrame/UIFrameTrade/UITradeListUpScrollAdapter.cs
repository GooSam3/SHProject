using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UITradeListUpScrollAdapter : OSA<BaseParamsWithPrefab, UITradeListUpViewsHolder>
{
	public SimpleDataHelper<ScrollTradeListUpData> Data
	{
		get; private set;
	}

	protected override UITradeListUpViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UITradeListUpViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UITradeListUpViewsHolder newOrRecycled)
	{
		ScrollTradeListUpData model = Data[newOrRecycled.ItemIndex];
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
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITradeListUpViewsHolder));
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

	public void SetData(Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollTradeListUpData>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직
		ZWebManager.Instance.WebGame.REQ_GetExchangeSellList((recvPacket, recvSellList) => {
			int sellfailcnt = 0;
			uint totalPrice = 0;

			float fRemainRefreshTime = 0;

			for (int i = 0; i < recvSellList.SellItemsLength; i++)
			{
				if (recvSellList.SellItems(i).Value.ExpireDt < TimeManager.NowSec)
					sellfailcnt++;
				else if (recvSellList.SellItems(i).Value.ExpireDt >= TimeManager.NowSec)
				{
					if (fRemainRefreshTime == 0)
						fRemainRefreshTime = recvSellList.SellItems(i).Value.ExpireDt - TimeManager.NowSec;
					else if (fRemainRefreshTime > (recvSellList.SellItems(i).Value.ExpireDt - TimeManager.NowSec))
						fRemainRefreshTime = recvSellList.SellItems(i).Value.ExpireDt - TimeManager.NowSec;
				}

				totalPrice += recvSellList.SellItems(i).Value.ItemTotalPrice;

				Data.List.Add(new ScrollTradeListUpData() { Data = new ExchangeItemData(recvSellList.SellItems(i).Value) });
			}

			if (UIManager.Instance.Find(out UIFrameTrade _trade))
				_trade.RefreshTextData(E_TradeMenu.Sell);

			if (fRemainRefreshTime != 0)
				Invoke(nameof(SetData), fRemainRefreshTime);

			Data.NotifyListChangedExternally();

			_callback?.Invoke();
		});
		#endregion
	}
}

public class ScrollTradeListUpData
{
	public ExchangeItemData Data;

	public void Reset(ScrollTradeListUpData _data)
	{
		this.Data = _data.Data;
	}
}

public class UITradeListUpViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image ItemIcon;			// 아이콘
	private Image ItemGradeBoard;	// 등급 빽판
	private Text ItemGrade;			// 강화 수치
	private Text ItemName;			// 이름
	private Text ItemNum;			// 수량
	private Text ItemInfo;			// 아이템 정보
	private Text ItemOptionInfo;    // 아이템 옵션 정보
	private Text RemainTime;      // 남은 시간
	private Button CancelButton;	// 판매 취소 버튼
	private Text ItemSellCost;		// 판매 금액
	private Image ItemSellIcon;     // 판매 금액 아이콘
	#endregion

	#region OSA System Variable
	ExchangeItemData ExchangeData;
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
		root.GetComponentAtPath("Enhance_Option/Txt_EnhanceOption", out ItemOptionInfo);
		root.GetComponentAtPath("Remain_Time/Txt_SellListNum", out RemainTime);
		root.GetComponentAtPath("Remain_Time/Bt_Cancel", out CancelButton);
		root.GetComponentAtPath("SellCost/Txt_Cost", out ItemSellCost);
		root.GetComponentAtPath("SellCost/Icon_Wealth", out ItemSellIcon);

		CancelButton.onClick.AddListener(ClickCancel);
	}

	public void ClickCancel()
	{
		UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
			_popup.Open(ZUIString.WARRING, string.Format(DBLocale.GetText("Cancellation_Exchange_Sales"), DBLocale.GetText(DBItem.GetItem(ExchangeData.ItemTId).ItemTextID)), new string[] { DBLocale.GetText("No_Text"), DBLocale.GetText("Yes_Text") }, new Action[] { delegate{ _popup.Close(); }, delegate {
					_popup.Close();
					ZWebManager.Instance.WebGame.REQ_ExchangeSellCancel(ExchangeData.ExchangeID, ExchangeData.ItemID, ExchangeData.ItemTId, (recvPacket, recvPacketMsg) => {
					if (UIManager.Instance.Find(out UIFrameTrade _trade))
					{
						_trade.ListUpScrollAdapter.SetData();
						_trade.ListUpInvenScrollAdapter.SetData(_trade.CurSearchInvenType, delegate { _trade.ListUpInvenScrollAdapter.RefreshData(); });
					}
				});
			} });
		});
	}

	public void UpdateViews(ScrollTradeListUpData _model)
	{
		var itemData = DBItem.GetItem(_model.Data.ItemTId);

		if (_model == null || itemData == null)
			return;

		ExchangeData = _model.Data;

		ItemIcon.sprite = UICommon.GetItemIconSprite(itemData.ItemID);
		ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(itemData.ItemID);
		ItemGrade.text = itemData.ItemUseType == GameDB.E_ItemUseType.Equip && itemData.Step > 0 ? "+" + itemData.Step.ToString() : string.Empty;
		ItemName.text = DBLocale.GetItemLocale(itemData);
		ItemNum.text = itemData.ItemUseType != GameDB.E_ItemUseType.Equip ? ExchangeData.ItemCnt.ToString() : string.Empty;
		//서버에서 제련 옵션 정보 주지 않는 것 같으니 확인 필요 (일단 string.empty로 작업)
		ItemInfo.text = string.Empty;
		ItemOptionInfo.text = string.Empty;
		RemainTime.text = TimeHelper.CompareNow(ExchangeData.ExpireDt);
		ItemSellIcon.sprite = UICommon.GetItemIconSprite(DBConfig.Diamond_ID);
		ItemSellCost.text = ExchangeData.ItemTotalPrice.ToString();
		ItemSellIcon.sprite = UICommon.GetItemIconSprite(DBConfig.Diamond_ID);
	}
}