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
using GameDB;

public class UICookInvenMaterialAdapter : GridAdapter<GridParams, UICookInvenViewsHolder>
{
	public SimpleDataHelper<ScrollCookInvenData> Data { get; private set; }

	protected override void UpdateCellViewsHolder(UICookInvenViewsHolder _holder)
	{
		if (_holder == null)
			return;

		ScrollCookInvenData data = Data[_holder.ItemIndex];
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
			GameObject CookInvenHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICookInvenViewsHolder));
			Parameters.Grid.CellPrefab = CookInvenHodler.GetComponent<RectTransform>();
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
			Data = new SimpleDataHelper<ScrollCookInvenData>(this);

		if (!IsInitialized)
			Initialize();

		#region 사용자 변경 로직
		ClearData();

		for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			var itemData = DBItem.GetItem(Me.CurCharData.InvenList[i].item_tid);

			if(itemData.ItemType == GameDB.E_ItemType.Ingredients)
			{
				var data = Data.List.Find(item => item.Item.item_tid == itemData.ItemID);

				if (data != null)
					data.Reset(new ScrollCookInvenData() { Item = Me.CurCharData.InvenList[i] });
				else
					if(Me.CurCharData.InvenList[i].cnt > 0)
						Data.List.Add(new ScrollCookInvenData() { Item = Me.CurCharData.InvenList[i] });
			}
		}
		#endregion

		Data.NotifyListChangedExternally();

		RefreshData();

		_callback?.Invoke();
	}
}

public class ScrollCookInvenData
{
	public ZItem Item;
	public bool isSelect;

	public void Reset(ScrollCookInvenData _data)
	{
		this.Item = _data.Item;
		this.isSelect = _data.isSelect;
	}
}

public class UICookInvenViewsHolder : CellViewsHolder
{
	#region OSA UI Variable
	private Image Icon = null;
	private Image GradeBoard = null;
	private Text GradeTxt = null;
	private Text NumTxt = null;
	private Image SelectImg = null;
	#endregion

	#region OSA System Variable
	[SerializeField] private ScrollCookInvenData Data;
	#endregion

	// Retrieving the views from the item's root GameObject
	public override void CollectViews()
	{
		base.CollectViews();

		views.GetComponentAtPath("ItemSlot_Share_Parts/Item_Icon", out Icon);
		views.GetComponentAtPath("ItemSlot_Share_Parts/Grade_Board", out GradeBoard);
		views.GetComponentAtPath("ItemSlot_Inven_Parts/Grade/Txt_Grade", out GradeTxt);
		views.GetComponentAtPath("ItemSlot_Inven_Parts/Num/Txt_Num", out NumTxt);
		views.GetComponentAtPath("ItemSlot_Inven_Parts/Check_Alram", out SelectImg);

		Icon.GetComponent<ZButton>().onClick.AddListener(ClickSlot);
	}

	public void ClickSlot()
	{
		if (Data.Item == null)
			return;

		if(UIManager.Instance.Find(out UIFrameCook _cook))
			_cook.OnSelectCombineCook(Data.Item);
	}

	public void UpdateViews(ScrollCookInvenData _model)
	{
		if (_model == null)
			return;

		Data = _model;

		var itemData = DBItem.GetItem(Data.Item.item_tid);

		if (itemData == null)
			return;

		SelectImg.gameObject.SetActive(Data.isSelect);
		Icon.sprite = UICommon.GetItemIconSprite(itemData.ItemID);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(itemData.ItemID);
		GradeTxt.text = itemData.ItemUseType == GameDB.E_ItemUseType.Equip && itemData.Step > 0 ? "+" + itemData.Step.ToString() : string.Empty;
		NumTxt.text = itemData.ItemUseType != GameDB.E_ItemUseType.Equip ? Data.Item.cnt.ToString() : string.Empty;
	}
}