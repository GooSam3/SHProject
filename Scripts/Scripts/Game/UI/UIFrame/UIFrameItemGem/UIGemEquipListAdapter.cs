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

public class UIGemEquipListAdapter : OSA<BaseParamsWithPrefab, UIGemEquipListHolder>
{
	public SimpleDataHelper<ScrollGemEquipItem> Data { get; private set; }

	protected override UIGemEquipListHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIGemEquipListHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	protected override void UpdateViewsHolder(UIGemEquipListHolder newOrRecycled)
	{
		ScrollGemEquipItem model = Data[newOrRecycled.ItemIndex];

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
			GameObject TradeHodler = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGemEquipListHolder));
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
			Data = new SimpleDataHelper<ScrollGemEquipItem>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직
		for(int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			var data = DBItem.GetItem(Me.CurCharData.InvenList[i].item_tid);

			if (data.SocketData.Count > 0)
				Data.List.Add(new ScrollGemEquipItem() { Item = Me.CurCharData.InvenList[i] });
		}
		#endregion

		Data.NotifyListChangedExternally();

		_callback?.Invoke();
	}
}

public class ScrollGemEquipItem
{
	public ZItem Item;
	public bool isSelect;

	public void Reset(ScrollGemEquipItem _data)
	{
		this.Item = _data.Item;
		this.isSelect = _data.isSelect;
	}
}

public class UIGemEquipListHolder : BaseItemViewsHolder
{
	#region OSA UI Variable
	private Image ItemIcon;
	private Image ItemGradeBoard;
	private Text ItemName;
	private Text ItemStep;
	private Text EquipGemCount;
	private Image ItemSelect;
	private ZButton Button;
	#endregion

	#region OSA System Variable
	private ZItem Item;
	private bool isSelect = false;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();

		root.GetComponentAtPath("Accessary_Slot/ItemSlot_Inven/ItemSlot_Share_Parts/Item_Icon", out ItemIcon);
		root.GetComponentAtPath("Accessary_Slot/ItemSlot_Inven/ItemSlot_Share_Parts/Grade_Board", out ItemGradeBoard);
		root.GetComponentAtPath("Accessary_Slot/Txt_ItemName", out ItemName);
		root.GetComponentAtPath("Accessary_Slot/Txt_ItemName/Txt_EnhanceNum", out ItemStep);
		root.GetComponentAtPath("Accessary_Slot/ItemSlot_Inven/ItemSlot_Inven_Parts/Gem_Slot/BG/Txt_GemNum", out EquipGemCount);
		root.GetComponentAtPath("Accessary_Slot/Img_SelectLine", out ItemSelect);
		root.GetComponentAtPath("Accessary_Slot/Img_Bg", out Button);

		Button.onClick.AddListener(OnSelect);
	}

	public void UpdateViews(ScrollGemEquipItem _model)
	{
		if (_model == null)
			return;

		Item = _model.Item;
		isSelect = _model.isSelect;

		var data = DBItem.GetItem(Item.item_tid);

		ItemIcon.sprite = UICommon.GetItemIconSprite(data.ItemID);
		ItemGradeBoard.sprite = UICommon.GetItemGradeSprite(data.ItemID);
		ItemName.text = DBLocale.GetItemLocale(data);
		//ItemStep.text = data.Step > 0 ? "+" + data.Step.ToString() : string.Empty;
		EquipGemCount.text = UICommon.GetEquipSocketCount(Item.Sockets).ToString();
		ItemSelect.gameObject.SetActive(isSelect);
	}

	public void OnSelect()
	{
		if (UIManager.Instance.Find(out UIFrameItemGem _gem))
			_gem.OnSelectEquipment(Item.item_id);
	}
}