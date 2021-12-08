using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZDefine;

public enum E_TradeSearchMainType
{
	Main = 0,
	Detail = 1,
}

public class UITradeScrollAdapter : OSA<BaseParamsWithPrefab, UITradeViewsHolder>
{
	public SimpleDataHelper<ScrollTradeData> Data
	{
		get; private set;
	}

	public E_TradeSearchMainType CurType = E_TradeSearchMainType.Main;
	public uint CurSearchItemId = 0;

	protected override UITradeViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UITradeViewsHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UITradeViewsHolder newOrRecycled)
	{
		ScrollTradeData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		CurSearchItemId = 0;
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
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITradeViewsHolder));
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

	public void SetData(E_TradeSearchMainType _type, uint _itemTid = 0, Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollTradeData>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직

		UIManager.Instance.Find(out UIFrameTrade _trade);
		if (_trade == null)
			return;

		_trade.ActiveSearchOptionButton(_type);

		CurType = _type;

		switch (_type)
		{
			// 분류 검색
			case E_TradeSearchMainType.Main:
				uint groupId = _trade.SearchGroupId;

				ZWebManager.Instance.WebGame.REQ_SearchExchangePrice(_trade.CurTradeSearchMainTab, _trade.CurTradeSearchSubTab, GameDB.E_CharacterType.All, 0, groupId, (recvPacket, recvPacketMsg) => {

					if (recvPacketMsg.SearchPricesLength == 0)
					{
						callback();
						return;
					}
						
					for (int i = 0; i < recvPacketMsg.SearchPricesLength; i++)
					{
						var data = Data.List.Find(item => item.ItemTid == recvPacketMsg.SearchPrices(i).Value.ItemTid);
						if (data == null)
							Data.List.Add(new ScrollTradeData() { ItemTid = recvPacketMsg.SearchPrices(i).Value.ItemTid, 
																  ItemGroupId = recvPacketMsg.SearchPrices(i).Value.ItemGroupId,
																  ItemMinPrice = recvPacketMsg.SearchPrices(i).Value.ItemMinPrice,
															      ItemResistCnt = recvPacketMsg.SearchPrices(i).Value.ItemRegistCnt});
						else
							data.Reset(new ScrollTradeData() { Data = data.Data });
					}

					callback();
				});
				break;

			// 세부 검색
			case E_TradeSearchMainType.Detail:

				var steps = _trade.SearchDetailInfoPopup.GetSelectEnhanceList();
				uint optionCnt = _trade.SearchDetailInfoPopup.EnchantOption.GetSelectToggleIndex();

				//uint step = 0;

				CurSearchItemId = _itemTid;

				List<uint> searchItem = new List<uint>();

				if (steps.Count > 0)
                {
					DBItem.GetAllItem(out var steptable);

					for(int i = 0; i < steps.Count; i++)
                    {
						var findItem = steptable.Find(item => item.GroupID == DBItem.GetItem(_itemTid).GroupID && item.Step == steps[i]);

						if (findItem != null)
							searchItem.Add(findItem.ItemID);
					}
					

					//if (searchItem != null)
					//	CurSearchItemId = searchItem.ItemID;
				}

				uint[] searchItemArray = new uint[searchItem.Count];

				for (int i = 0; i < searchItemArray.Length; i++)
					searchItemArray[i] = searchItem[i];

				ZWebManager.Instance.WebGame.REQ_SearchExchangeList(searchItemArray, 0, E_ExchangeSortType.TotalPriceDesc, optionCnt,(recvPacket, recvPacketMsg) =>
				{
					if (recvPacketMsg.SearchItemsLength == 0)
					{
						callback();
						return;
					}
						
					for (int i = 0; i < recvPacketMsg.SearchItemsLength; i++)
					{
						var data = Data.List.Find(item => item.Data.ItemTId == recvPacketMsg.SearchItems(i).Value.ItemTid);
						if (data == null)
							Data.List.Add(new ScrollTradeData() { Data = new ExchangeItemData(recvPacketMsg.SearchItems(i).Value)});
						else
							data.Reset(new ScrollTradeData() { Data = data.Data });
					}

					callback();
				});
				break;
		}

		void callback()
		{
			_callback?.Invoke();

			Data.NotifyListChangedExternally();

			_trade.RefreshTextData(E_TradeMenu.Search);

			RefreshData();
		}
		#endregion
	}
}

public class ScrollTradeData
{
	// Main Tab
	public uint ItemTid;           // 거래소 아이템 테이블 ID
	public uint ItemGroupId;       // 거래소 아이템 테이블 그룹 ID
	public uint ItemResistCnt;     // 거래소에 등록된 아이템 개수
    public float ItemMinPrice;   // 거래소 등록된 아이템 최저판매 가격

	// Sub Tab
	public ExchangeItemData Data;

	// Buy Option
	public bool IsCheck = false;

	public void Reset(ScrollTradeData _data)
	{
		ItemTid = _data.ItemTid;
		ItemGroupId = _data.ItemGroupId;
		ItemResistCnt = _data.ItemResistCnt;
		ItemMinPrice = _data.ItemMinPrice;
		Data = _data.Data;
		IsCheck = _data.IsCheck;
	}
}

public class UITradeViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private ZButton SlotBoard;
	private ZButton SlotBoardToggle;
	private ZToggle BuyCheckOn;
	private Image ItemIcon;
	private Image ItemGradeBoard;
	private Image ItemCostIcon;
	private Text ItemGradeTxt;
	private Text ItemName;
	private Text ItempOption;
	private Text ItemCost;
	private Text ItemCount;
	private Text ItemRemainTime;
	private RectTransform BuyCheckBox;
	#endregion

	#region OSA System Variable
	private ScrollTradeData ExchangeData;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Share_Parts/Item_Icon", out ItemIcon);
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Share_Parts/Grade_Board", out ItemGradeBoard);
		root.GetComponentAtPath("SellCost/Txt_Cost/Icon_Wealth", out ItemCostIcon);
		root.GetComponentAtPath("Item/Txt_Box/Txt_ItemName", out ItemName);
		root.GetComponentAtPath("Item/Txt_Box/Txt_ItemOption", out ItempOption);
		root.GetComponentAtPath("SellCost/Txt_Cost", out ItemCost);
		root.GetComponentAtPath("RemainTime/Txt_Time", out ItemRemainTime);
		root.GetComponentAtPath("SellList/Txt_SellListNum", out ItemCount);
		root.GetComponentAtPath("Item/ItemSlot_Inven/ItemSlot_Inven_Parts/Grade/Txt_Grade", out ItemGradeTxt);
		root.GetComponentAtPath("Img_DetailInfo_TouchField", out SlotBoard);
		root.GetComponentAtPath("Item/MultiBuy/Bt_Check_Box", out BuyCheckBox);
		root.GetComponentAtPath("Item/MultiBuy/Img_DetailInfo_TouchField_Toggle", out SlotBoardToggle);
		root.GetComponentAtPath("Item/MultiBuy/Bt_Check_Box", out BuyCheckOn);

		SlotBoard.onClick.AddListener(OnClickItem);
		SlotBoardToggle.onClick.AddListener(OnClickBuy);
	}

	public void UpdateViews(ScrollTradeData _model)
	{
		if (_model == null)
			return;

		ExchangeData = _model;

		if (!UIManager.Instance.Find(out UIFrameTrade _trade))
			return;

		BuyCheckBox.gameObject.SetActive(_trade.ScrollAdapter.CurType == E_TradeSearchMainType.Detail);

		ItempOption.text = string.Empty;
		ItemCount.text = string.Empty;
		ItemRemainTime.text = string.Empty;
		ItemGradeTxt.text = string.Empty;
		ItemCostIcon.sprite = UICommon.GetItemIconSprite(DBConfig.Diamond_ID);
		
		switch (_trade.ScrollAdapter.CurType)
		{
			case E_TradeSearchMainType.Main:
				var itemPriceData = DBItem.GetItem(ExchangeData.ItemTid);
				ItemIcon.sprite = UICommon.GetItemIconSprite(itemPriceData.ItemID);
				ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(itemPriceData.ItemID);
				ItemName.text = DBLocale.GetText(itemPriceData.ItemTextID);
				ItemCost.text = ExchangeData.ItemMinPrice.ToString(); // 최저 금액
				ItemCount.text = ExchangeData.ItemResistCnt.ToString(); // 판매 등록된 수
				break;

			case E_TradeSearchMainType.Detail:
				var itemDetailData = DBItem.GetItem(ExchangeData.Data.ItemTId);
				ItemIcon.sprite = UICommon.GetItemIconSprite(itemDetailData.ItemID);
				ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(itemDetailData.ItemID);
				ItemName.text = DBLocale.GetText(itemDetailData.ItemTextID);
				ItemCost.text = ExchangeData.Data.ItemTotalPrice.ToString(); // 판매 가격
				ItemCount.text = ExchangeData.Data.ItemCnt.ToString();
				ItemGradeTxt.text = itemDetailData.Step > 0 ? "+" + itemDetailData.Step.ToString() : string.Empty;

				if (ExchangeData.Data.ExpireDt <= TimeManager.NowSec)
					ItemRemainTime.text = "시간 만료";
				else
					ItemRemainTime.text = TimeHelper.CompareNow(ExchangeData.Data.ExpireDt);
				
				// 제련 옵션
				for (int i = 0; i < ExchangeData.Data.ItemOptions.Length; i++)
				{
					var ability = DBAbility.GetAction(DBResmelting.GetResmeltingAbilityActionId( ExchangeData.Data.ItemOptions[i]));
					
					ItempOption.text += ability != null ? " " + DBLocale.GetText(DBAbility.GetAbility(ability.AbilityID_01).StringName) + ability.AbilityPoint_01_Min : " - ";
					ItempOption.text += i + 1 == ExchangeData.Data.ItemOptions.Length ? "/" : ",";
				}

				BuyCheckOn.SelectToggleSingle(ExchangeData.IsCheck);
				break;
		}
	}

	public void OnClickBuy()
	{
		if (UIManager.Instance.Find(out UIFrameTrade _trade))
		{
			_trade.CheckMultiBuyList(ExchangeData.Data);
			RefreshMultiBuyListHolder();
		}
	}

	private void RefreshMultiBuyListHolder()
	{
		if (!UIManager.Instance.Find(out UIFrameTrade _trade))
			return;

		if (_trade.ScrollAdapter.CurType != E_TradeSearchMainType.Detail)
			return;

		var exchangeitem = _trade.MultiBuyList.Find(item => item.ExchangeID == ExchangeData.Data.ExchangeID);

		ExchangeData.IsCheck = exchangeitem != null;
		_trade.ScrollAdapter.RefreshData();
	}

	public void OnClickItem()
	{
		if (!UIManager.Instance.Find(out UIFrameTrade _trade))
			return;

		switch (_trade.ScrollAdapter.CurType)
		{
			// 목록 확인
			case E_TradeSearchMainType.Main:
				_trade.ScrollAdapter.SetData(E_TradeSearchMainType.Detail, ExchangeData.ItemTid);
				break;

			// 구입
			case E_TradeSearchMainType.Detail:
				ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
				{
					UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

					if (obj != null)
					{
						if(_trade.GetInfoPopup())
							_trade.RemoveInfoPopup();
						
						_trade.SetInfoPopup(obj);

						obj.transform.SetParent(_trade.gameObject.transform);
						obj.Initialize(ExchangeData.Data);
					}
				});
				break;
		}
	}
}