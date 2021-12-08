using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICheatDataHolder : ZAdapterHolderBase<OSA_CheatData>
{
	private UICheatListItem listItem;

	public override void CollectViews()
	{
		base.CollectViews();

		listItem = root.GetComponent<UICheatListItem>();
	}

	public void SetAction(Action<OSA_CheatData> _onClickLeft, Action<OSA_CheatData> _onClickRight)
	{
		listItem.SetAction(_onClickLeft, _onClickRight);
	}


	public override void SetSlot(OSA_CheatData data)
	{
		listItem.SetSlot(data);
	}

}

public class UICheatListItem : MonoBehaviour
{
	[SerializeField] private Image imgFavorite;
	[SerializeField] private Text txtTitle;
	[SerializeField] private Text txtNum;

	[SerializeField] private GameObject objSpawn;

	[SerializeField] private Image imgBG;

	private Action<OSA_CheatData> onClickLeft;
	private Action<OSA_CheatData> onClickRight;

	public OSA_CheatData Data { get; private set; }

	private bool isInitialized = false;

	// 타입별로 한번만 초기화됨(데이터 섞일일 없음)
	private void Initialize(E_CheatDataType type)
	{
		imgFavorite.gameObject.SetActive(false);
		txtTitle.gameObject.SetActive(true);
		txtNum.gameObject.SetActive(false);
		objSpawn.gameObject.SetActive(false);	

		switch (type)
		{
			case E_CheatDataType.Item_Shop:
				imgFavorite.gameObject.SetActive(true);
				break;
			case E_CheatDataType.Item_Wish:
				txtNum.gameObject.SetActive(true);
				break;
			case E_CheatDataType.Monster:
				imgFavorite.gameObject.SetActive(true);
				objSpawn.gameObject.SetActive(true);
				break;
		}
	}

	public void SetSlot(OSA_CheatData data)
	{
		if (isInitialized == false)
			Initialize(data.type);

		Data = data;

		switch (Data.type)
		{
			case E_CheatDataType.Item_Tab:
			case E_CheatDataType.Monster_Tab:
				SetTab();
				break;
			case E_CheatDataType.Item_Shop:
				SetShopItem();
				break;
			case E_CheatDataType.Item_Wish:
				SetWishItem();
				break;

			case E_CheatDataType.Monster:
				SetMonster();
				break;
		}
	}

	private void SetTab()
	{
		if (Data.type == E_CheatDataType.Item_Tab)
		{
			if (Data.itemType == 0)
				txtTitle.text = "전체";
			else
				txtTitle.text = DBLocale.GetText(Data.itemType.ToString());
		}
		else
		{
			if (Data.monsterType < 0)
				txtTitle.text = "전체";
			else
				txtTitle.text = DBLocale.GetText(Data.monsterType.ToString());
		}

		imgBG.color = Data.isSelected ? Color.cyan : Color.white;
	}

	private void SetShopItem()
	{
		if (Data.itemTable == null)
		{
			imgFavorite.color = Color.white;
			txtTitle.text = "데이터 없음";
		}
		else
		{
			bool isFavorite = C_CheatFavoriteHelper.HasItemValue(Data.itemTable);

			imgFavorite.color = isFavorite ? Color.yellow : Color.white;
			txtTitle.text = UICommon.GetItemText(Data.itemTable);
		}
	}

	private void SetWishItem()
	{
		if (Data.itemTable == null)
		{
			txtTitle.text = "데이터 없음";
			txtNum.text = "NULL";
		}
		else
		{
			txtTitle.text = UICommon.GetItemText(Data.itemTable);
			txtNum.text = $"{Data.count} 개";
		}
	}

	private void SetMonster()
	{
		if(Data.monsterTable == null)
		{
			txtTitle.text = "데이터 없음";
			imgFavorite.color = Color.white;
		}
		else
		{
			bool isFavorite = C_CheatFavoriteHelper.HasMonsterValue(Data.monsterTable);

			imgFavorite.color = isFavorite ? Color.yellow : Color.white;
			txtTitle.text = DBLocale.GetText(Data.monsterTable.MonsterTextID);
		}
	}

	public void SetAction(Action<OSA_CheatData> _onClickLeft, Action<OSA_CheatData> _onClickRight)
	{
		onClickLeft = _onClickLeft;
		onClickRight = _onClickRight;
	}

	public void OnClickFavorite()
	{
		if (Data.type == E_CheatDataType.Item_Shop)
		{

			var hasValue = C_CheatFavoriteHelper.HasItemValue(Data.itemTable);
			if (hasValue == false)
				C_CheatFavoriteHelper.AddFavoriteItem(Data.itemTable);
			else
				C_CheatFavoriteHelper.RemoveFavoriteItem(Data.itemTable);
		}
		else
		{
			var hasValue = C_CheatFavoriteHelper.HasMonsterValue(Data.monsterTable);
			if (hasValue == false)
				C_CheatFavoriteHelper.AddFavoriteMonster(Data.monsterTable);
			else
				C_CheatFavoriteHelper.RemoveFavoriteMonster(Data.monsterTable);
		}

		SetSlot(Data);
	}

	public void OnClickLeft()
	{
		if (Data.type == E_CheatDataType.Monster)
			return;

		if (Input.GetKey(KeyCode.LeftControl))
			onClickRight?.Invoke(Data);
		else
			onClickLeft?.Invoke(Data);
	}

	public void OnClickSpawn()
	{
		onClickLeft?.Invoke(Data);
	}
}
