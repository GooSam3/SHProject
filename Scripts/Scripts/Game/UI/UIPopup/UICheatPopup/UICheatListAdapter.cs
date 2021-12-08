using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICheatListAdapter : ZScrollAdapterBase<OSA_CheatData, UICheatDataHolder>
{
	protected override string AddressableKey => nameof(UICheatListItem);

	private Action<OSA_CheatData> onClickLeft = null;
	private Action<OSA_CheatData> onClickRight = null;

	public OSA_CheatData selectedData { get; private set; } = null;

	public void Initialize(Action<OSA_CheatData> _onClickLeft, Action<OSA_CheatData> _onClickRight = null)
	{
		onClickLeft = _onClickLeft;
		onClickRight = _onClickRight;

		Initialize();
	}

	protected override void OnCreateHolder(UICheatDataHolder holder)
	{
		base.OnCreateHolder(holder);
		holder.SetAction((data)=>OnClickListItem(data,true), onClickRight);
	}

	public void SetSelectedData(OSA_CheatData data)
	{
		if (selectedData != null)
			selectedData.isSelected = false;

		selectedData = data;

		if (selectedData != null)
		{
			selectedData.isSelected = true;
		}

		RefreshData();
	}

	public void SelectFirst(bool isInvoke = true)
	{
		if (Data.Count > 0)
		{
			OnClickListItem(Data.List[0], isInvoke);
		}
	}

	public void OnClickListItem(OSA_CheatData data, bool isInvoke = true)
	{
		if (data.isSelectable)
			SetSelectedData(data);

		if(isInvoke)
			onClickLeft?.Invoke(data);
	}
}

public enum E_CheatDataType
{
	Item_Tab,	// 아이템 탭
	Item_Shop,	// 아이템(상점)
	Item_Wish,	// 아이템(장바구니)
	Monster_Tab,// 몬스터 탭
	Monster,	// 몬스터(스폰)
}

public class OSA_CheatData
{
	public E_CheatDataType type;

	public Item_Table itemTable;			//item
	public ulong count;                     //item

	public Monster_Table monsterTable;		//

	public E_ItemType itemType;             //tab
	public E_MonsterType monsterType;		//tab

	public bool isSelectable = false;		//tab
	public bool isSelected = false;			//tab
}