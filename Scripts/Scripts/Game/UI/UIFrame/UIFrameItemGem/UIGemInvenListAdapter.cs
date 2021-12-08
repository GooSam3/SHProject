using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using ZDefine;
using ZNet.Data;

public class UIGemInvenListAdapter : GridAdapter<GridParams, UIGemInvenListHolder>
{
	public SimpleDataHelper<ScrollGemData> Data { get; private set; }

	protected override void UpdateCellViewsHolder(UIGemInvenListHolder newOrRecycled)
	{
		if (newOrRecycled == null)
			return;

		ScrollGemData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
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
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGemInvenListHolder));
			Parameters.Grid.CellPrefab = TradeHodler.GetComponent<RectTransform>();
			Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
			Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
			Parameters.Grid.CellPrefab.transform.localPosition = Vector3.zero;
			Parameters.Grid.CellPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.Grid.CellPrefab != null);
		}

		if (!IsInitialized)
			Init();
	}

	private void ClearData()
	{
		Data.List.Clear();
	}

	public void SetData(Action _callback = null)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollGemData>(this);

		if (!IsInitialized)
			Initialize();

		#region 사용자 변경 로직
		ClearData();

		for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			var data = DBItem.GetItem(Me.CurCharData.InvenList[i].item_tid);

			if (data.EquipSlotType == GameDB.E_EquipSlotType.Gem && Me.CurCharData.InvenList[i].cnt > 0)
				Data.List.Add(new ScrollGemData() { Item = Me.CurCharData.InvenList[i] });

		}
		#endregion

		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}
}

public class ScrollGemData
{
	public ZItem Item;
	public bool isSelect;

	public void Reset(ScrollGemData _data)
	{
		this.Item = _data.Item;
		this.isSelect = _data.isSelect;
	}
}

public class UIGemInvenListHolder : CellViewsHolder
{
	#region OSA UI Variable
	private Image Icon = null;
	private Image GradeBoard = null;
	private Text NumTxt = null;
	private Image SelectImg = null;
	#endregion

	#region OSA System Variable
	[SerializeField] private ScrollGemData Data;
	#endregion


	public override void CollectViews()
	{
		base.CollectViews();

		views.GetComponentAtPath("ItemSlot_Inven/ItemSlot_Share_Parts/Item_Icon", out Icon);
		views.GetComponentAtPath("ItemSlot_Inven/ItemSlot_Share_Parts/Grade_Board", out GradeBoard);
		views.GetComponentAtPath("ItemSlot_Inven/ItemSlot_Inven_Parts/Num/Txt_Num", out NumTxt);
		views.GetComponentAtPath("ItemSlot_Inven/ItemSlot_Inven_Parts/Select/Img_Select", out SelectImg);

		Icon.GetComponent<ZButton>().onClick.AddListener(OnSelect);
	}

	public void UpdateViews(ScrollGemData _data)
	{
		if (_data == null)
			return;

		Data = _data;

		var data = DBItem.GetItem(Data.Item.item_tid);

		SelectImg.gameObject.SetActive(Data.isSelect);

		Icon.sprite = UICommon.GetItemIconSprite(data.ItemID);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(data.ItemID);
		NumTxt.text = Me.CurCharData.GetItem(Data.Item.item_tid).cnt.ToString();
	}

	public void OnSelect()
	{
		if (!UIManager.Instance.Find(out UIFrameItemGem _gem))
			return;

		_gem.OnSelectInvenGem(Data.Item.item_id);

		if(_gem.InfoPopup != null)
		{
			_gem.InfoPopup.Initialize(E_ItemPopupType.GemEquip, Data.Item);
		}
		else
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
			{
				UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

				if (obj != null)
				{
					_gem.InfoPopup = obj;
					obj.transform.SetParent(_gem.gameObject.transform);
					obj.Initialize(E_ItemPopupType.GemEquip, Data.Item);
				}
			});
		}
	}
}