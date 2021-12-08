using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UITradeListUpInvenScrollAdapter : GridAdapter<GridParams, UITradeListUpInvenViewsHolder>
{
	public SimpleDataHelper<ScrollTradeListUpInvenData> Data
	{
		get; private set;
	}

	protected override void UpdateCellViewsHolder(UITradeListUpInvenViewsHolder _holder)
	{
		if (_holder == null)
			return;

		ScrollTradeListUpInvenData data = Data[_holder.ItemIndex];
		_holder.UpdateViews(data);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.CellsCount; i++)
			UpdateCellViewsHolder(base.GetCellViewsHolder(i));
	}

	public void Initialize()
	{
		if (Parameters.Grid.CellPrefab == null)
		{
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITradeListUpInvenViewsHolder));
			Parameters.Grid.CellPrefab = TradeHodler.GetComponent<RectTransform>();
			Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
			Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
			Parameters.Grid.CellPrefab.transform.localPosition = Vector3.zero;
			Parameters.Grid.CellPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.Grid.CellPrefab != null);
		}

		if(!IsInitialized)
			Init();
	}

	private void ClearData()
	{
		Data.List.Clear();
	}

	public void SetData(E_InvenSortType _sortType, Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollTradeListUpInvenData>(this);

		if (!IsInitialized)
			Initialize();

		#region 사용자 변경 로직
		ClearData();

		for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			var itemData = DBItem.GetItem(Me.CurCharData.InvenList[i].item_tid);

			if (itemData.LimitType.HasFlag(E_LimitType.Exchange) && itemData.BelongType == E_BelongType.None)
			{
				var itemdata = Data.List.Find(item => item.Item.item_id == Me.CurCharData.InvenList[i].item_id && item.Item.netType == Me.CurCharData.InvenList[i].netType);

				switch (_sortType)
				{
					case E_InvenSortType.All:
						callBack(itemdata, Me.CurCharData.InvenList[i]);
						break;

					case E_InvenSortType.Equipment:
						if(itemData.ItemUseType == E_ItemUseType.Equip)
						{
							callBack(itemdata, Me.CurCharData.InvenList[i]);
						}
						break;

					case E_InvenSortType.ETC:
						if (itemData.ItemUseType != E_ItemUseType.Equip)
						{
							callBack(itemdata, Me.CurCharData.InvenList[i]);
						}
						break;

						void callBack(ScrollTradeListUpInvenData _data, ZItem _item)
						{
							if (_item.cnt == 0)
								return;

							if (_data == null)
								Data.List.Add(new ScrollTradeListUpInvenData() { Item = _item });
							else
								_data.Reset(new ScrollTradeListUpInvenData() { Item = _item });
						}
				}
			}
		}
		#endregion

		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}
}

public class ScrollTradeListUpInvenData
{
	public ZItem Item;

	public void Reset(ScrollTradeListUpInvenData _data)
	{
		this.Item = _data.Item;	
	}
}

public class UITradeListUpInvenViewsHolder : CellViewsHolder
{
	#region OSA UI Variable
	private Image Icon = null;
	private Image GradeBoard = null;
	private Text GradeTxt = null;
	private Text NumTxt = null;
	#endregion

	#region OSA System Variable
	[SerializeField] private ScrollTradeListUpInvenData Data;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();
		views.GetComponentAtPath("ItemSlot_Share_Parts/Item_Icon", out Icon);
		views.GetComponentAtPath("ItemSlot_Share_Parts/Grade_Board", out GradeBoard);
		views.GetComponentAtPath("ItemSlot_Inven_Parts/Grade/Txt_Grade", out GradeTxt);
		views.GetComponentAtPath("ItemSlot_Inven_Parts/Num/Txt_Num", out NumTxt);

		Icon.GetComponent<ZButton>().onClick.AddListener(ClickSlot);
	}

	public void ClickSlot()
	{
		if (Data.Item == null)
			return;

		ZWebManager.Instance.WebGame.REQ_GetExchangePriceInfo(Data.Item.item_tid, (recvPacket, recvPacketMsg) => {
			if (UIManager.Instance.Find(out UIFrameTrade _trade))
			{
				_trade.SellInfoPopup.gameObject.SetActive(true);
				_trade.SellInfoPopup.Initialize(Data.Item, recvPacketMsg.ItemMinPrice, recvPacketMsg.ItemMaxPrice);
			}
		});
	}

	public void UpdateViews(ScrollTradeListUpInvenData _model)
	{
		if (_model == null)
			return;

		Data = _model;
		var itemData = DBItem.GetItem(Data.Item.item_tid);

		if (itemData == null)
			return;

		Icon.sprite = UICommon.GetItemIconSprite(itemData.ItemID);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(itemData.ItemID);
		GradeTxt.text = itemData.ItemUseType == GameDB.E_ItemUseType.Equip && itemData.Step > 0 ? "+" + itemData.Step.ToString() : string.Empty;
		NumTxt.text = itemData.ItemUseType != GameDB.E_ItemUseType.Equip ? Data.Item.cnt.ToString() : string.Empty;
	}
}