using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using ZNet.Data;

public class UIGodLandLocalDetailEnemyItem : MonoBehaviour
{
	[SerializeField] private UIGodLandLocalDetailEnemyItemSlot mySlot;
	[SerializeField] private UIGodLandLocalDetailEnemyItemSlot ownerSlot;
	[SerializeField] private ZText actionButtonName;
	[SerializeField] private ZText productionName;
	[SerializeField] private ZText productionAvgCount;
	[SerializeField] private ZImage productionAvgImage;
	[SerializeField] private ZText productionMaxCount;
	[SerializeField] private ZImage productionMaxImage;
	[SerializeField] private ZText enterCost;
	[SerializeField] private ZImage currencyIcon;

	private Action<E_TargetType, uint> actionCallback;

	private uint godLandTid;

	public void Initialize(Action<E_TargetType, uint> _actionCallback)
	{
		productionName.text = DBLocale.GetText("GodLand_Production_Info");
		enterCost.text = $"{DBConfig.GodLand_Require_Stamina}";
		currencyIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_occupy_01");

		actionCallback = _actionCallback;
	}

	public void Show(GodLandSpotInfoConverted _ownerData, GodLandSpotInfoConverted myData )
	{
		gameObject.SetActive(true);

		godLandTid = _ownerData.GodLandTid;

		actionButtonName.text = DBLocale.GetText("GodLand_Robbery_Button"); //강탈

		mySlot.SetData(myData, myData.Atk > _ownerData.Atk, myData.Def > _ownerData.Def, myData.Mdef > _ownerData.Mdef);
		ownerSlot.SetData(_ownerData, myData.Atk < _ownerData.Atk, myData.Def < _ownerData.Def, myData.Mdef < _ownerData.Mdef);

		//생산아이템 표시
		var itemTable = DBItem.GetItem(_ownerData.GodLandTable.ProductionItemID);
		if (itemTable != null) {
			productionAvgImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
			productionMaxImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);

			string amountTile = DBLocale.GetText("GodLand_Time_Production"); //시간당 생산량
			float amount = ((float)3600 / _ownerData.GodLandTable.ProductionTime) * _ownerData.GodLandTable.ProductionItemCount;
			productionAvgCount.text = string.Format("{0}:{1:0}", amountTile, amount);

			amountTile = DBLocale.GetText("GodLand_Max_Production"); //최대 생산량
			productionMaxCount.text = $"{amountTile}:{ _ownerData.GodLandTable.ProductionItemCountMax}";
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"해당재화에 해당하는 아이템을 찾을수 없다, itemTid:{_ownerData.GodLandTable.ProductionItemID}");
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void OnClickAction()
	{
		actionCallback?.Invoke(E_TargetType.Enemmy, godLandTid);
	}
}