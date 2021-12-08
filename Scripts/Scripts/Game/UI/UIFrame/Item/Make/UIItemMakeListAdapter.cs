using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using ZDefine;
using ZNet.Data;
using GameDB;

public class UIItemMakeListAdapter : OSA<BaseParamsWithPrefab, UIItemMakeListHolder>
{
	public SimpleDataHelper<ScrollMakeItem> Data { get; private set; }

	protected override UIItemMakeListHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIItemMakeListHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	protected override void UpdateViewsHolder(UIItemMakeListHolder newOrRecycled)
	{
		ScrollMakeItem model = Data[newOrRecycled.ItemIndex];

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
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIItemMakeListHolder));
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
			Data = new SimpleDataHelper<ScrollMakeItem>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직
		if (!UIManager.Instance.Find(out UIFrameItemMake _make))
			return;

		List<Make_Table> makeTable = new List<Make_Table>();

		switch(_make.SelectMakeType)
		{
			case E_MakeType.Weapon:
				var weaponList = DBMake.GetMakeTypeDatas(E_MakeType.Weapon, _make.SelectMakeSubType);
				var defenseList = DBMake.GetMakeTypeDatas(E_MakeType.DefenseEquip, _make.SelectMakeSubType);

				if (weaponList != null) makeTable.AddRange(weaponList);
				if (defenseList != null) makeTable.AddRange(defenseList);
				break;

			default:
				var makeList = DBMake.GetMakeTypeDatas(_make.SelectMakeType, _make.SelectMakeSubType);
				if(makeList != null) makeTable.AddRange(makeList);
				break;
		}

		for (int i = 0; i < makeTable.Count; i++)
		{
			if(DBItem.GetItem(makeTable[i].SuccessGetItemID).UseCharacterType == _make.SelectMakeClassType || _make.SelectMakeClassType == E_CharacterType.All)
				Data.List.Add(new ScrollMakeItem() { Item = makeTable[i] });
		}
		#endregion

		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}
}

public class ScrollMakeItem
{
	public Make_Table Item;
	public bool isSelect;

	public void Reset(ScrollMakeItem _data)
	{
		this.Item = _data.Item;
		this.isSelect = _data.isSelect;
	}
}

public class UIItemMakeListHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image ItemIcon;
	private Image ItemGradeBoard;
	private Text ItemName;
	private Image ItemClassIcon;
	private Image ItemSelect;
	private ZButton Button;
	#endregion

	#region OSA System Variable
	private Make_Table Item;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();

		root.GetComponentAtPath("ItemListSlot_Inven/UIItemSlot/Views/ItemSlot_Share_Parts/Item_Icon", out ItemIcon);
		root.GetComponentAtPath("ItemListSlot_Inven/UIItemSlot/Views/ItemSlot_Share_Parts/Grade_Board", out ItemGradeBoard);
		root.GetComponentAtPath("ItemListSlot_Inven/Txt_ItemName", out ItemName);
		root.GetComponentAtPath("ItemListSlot_Inven/Icon_Class", out ItemClassIcon);
		root.GetComponentAtPath("ItemListSlot_Inven/Img_Select", out ItemSelect);
		root.GetComponentAtPath("ItemListSlot_Inven/Img_Bg", out Button);

		Button.onClick.AddListener(OnSelect);
	}

	public void UpdateViews(ScrollMakeItem _model)
	{
		if (_model == null)
			return;

		Item = _model.Item;

		var data = DBItem.GetItem(Item.SuccessGetItemID);

		ItemIcon.sprite = UICommon.GetItemIconSprite(data.ItemID);
		ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(data.ItemID);
		ItemName.text = DBLocale.GetItemLocale(data);
		ItemClassIcon.sprite = UICommon.GetClassIconSprite(DBItem.GetItem(data.ItemID).UseCharacterType, UICommon.E_SIZE_OPTION.Small);
		ItemSelect.gameObject.SetActive(_model.isSelect);
	}

	public void OnSelect()
	{
		if (UIManager.Instance.Find(out UIFrameItemMake _make))
			_make.OnSelectMakeItem(Item);
	}
}