using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIPopupEquipPromotionResult : UIPopupBase
{
	[SerializeField] private GameObject targetObj;
	[SerializeField] private Image imgItem;
	[SerializeField] private Image imgGrade;

	[SerializeField] private Text txtGrade;
	[SerializeField] private Text txtItemName;

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
		targetObj.SetActive(false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		targetObj.SetActive(false);
	}

	public void PlayFX(Item_Table _itemDest)
	{
		imgItem.sprite = UICommon.GetSprite(_itemDest.IconID);
		imgGrade.sprite = UICommon.GetGradeSprite(_itemDest.Grade);
		txtItemName.text = DBUIResouce.GetItemGradeFormat(DBLocale.GetText(_itemDest.ItemTextID), _itemDest.Grade);
		txtGrade.text = DBUIResouce.GetTierText(_itemDest.Grade);

		targetObj.SetActive(true);
	}

	public void OnClickClose()
	{
		UIManager.Instance.Close<UIPopupEquipPromotionResult>(true);
	}
}
