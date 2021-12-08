using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using ZNet.Data;

public class UICookRecipeListAdapter : OSA<BaseParamsWithPrefab, UICookRecipeListViewsHolder>
{
	public SimpleDataHelper<ScrollRecipeData> Data { get; private set; }

	protected override UICookRecipeListViewsHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UICookRecipeListViewsHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	protected override void UpdateViewsHolder(UICookRecipeListViewsHolder newOrRecycled)
	{
		ScrollRecipeData model = Data[newOrRecycled.ItemIndex];

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
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICookRecipeListViewsHolder));
			Parameters.ItemPrefab = TradeHodler.GetComponent<RectTransform>();
			Parameters.ItemPrefab.SetParent(GetComponent<Transform>());
			Parameters.ItemPrefab.transform.localScale = Vector2.one;
			Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
			Parameters.ItemPrefab.gameObject.SetActive(false);
			gameObject.SetActive(Parameters.ItemPrefab != null);
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
			Data = new SimpleDataHelper<ScrollRecipeData>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직
		if (!UIManager.Instance.Find(out UIFrameCook _cook))
			return;

		for (int i = 0; i < Me.CurCharData.CookRecipeList.Count; i++)
		{
			Data.List.Add(new ScrollRecipeData() { CookTid = Me.CurCharData.CookRecipeList[i].CookTid });
		}
		#endregion

		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}
}

public class ScrollRecipeData
{
	public uint CookTid;
	public bool isSelect;
}

public class UICookRecipeListViewsHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image ItemIcon;
	private Image ItemGradeBoard;
	private Text ItemName;
	private Image ItemSelect;
	private ZButton Button;
	private Image BelongIcon = null;
	private Text BelongTxt = null;
	#endregion

	#region OSA System Variable
	private Cooking_Table Item;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();

		root.GetComponentAtPath("ItemListSlot_Inven/UIItemSlot/Views/ItemSlot_Share_Parts/Item_Icon", out ItemIcon);
		root.GetComponentAtPath("ItemListSlot_Inven/UIItemSlot/Views/ItemSlot_Share_Parts/Grade_Board", out ItemGradeBoard);
		root.GetComponentAtPath("ItemListSlot_Inven/Txt_ItemName", out ItemName);
		root.GetComponentAtPath("ItemListSlot_Inven/Img_Select", out ItemSelect);
		root.GetComponentAtPath("ItemListSlot_Inven/Img_Bg", out Button);
		root.GetComponentAtPath("BelongTxt_Alarm/Txt_Belong/Icon", out BelongIcon);
		root.GetComponentAtPath("BelongTxt_Alarm/Txt_Belong", out BelongTxt);

		Button.onClick.AddListener(OnSelect);
	}

	public void UpdateViews(ScrollRecipeData _model)
	{
		if (_model == null)
			return;

		Item = DBCooking.Get(_model.CookTid);

		var data = DBItem.GetItem(Item.SuccessGetItemID);

		ItemIcon.sprite = UICommon.GetItemIconSprite(data.ItemID);
		ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(data.ItemID);
		ItemName.text = DBLocale.GetItemLocale(data);
		ItemSelect.gameObject.SetActive(_model.isSelect);
		BelongIcon.gameObject.SetActive(data.BelongType != E_BelongType.Belong);
		BelongTxt.text = data.BelongType == E_BelongType.Belong ? "귀속" : "비귀속";
	}

	public void OnSelect()
	{
		if (UIManager.Instance.Find(out UIFrameCook _cook))
			_cook.OnSelectCook(Item);
	}
}