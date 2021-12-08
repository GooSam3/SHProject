using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GameDB;
using ZNet.Data;

public class UIGodLandMapWorldItem : MonoBehaviour
{
	[SerializeField] GameObject mapPointLockObj;
	[SerializeField] GameObject mapPointObj;
	[SerializeField] private ZText levelText1;
	[SerializeField] private ZText levelText2;

	[SerializeField] private uint godLandGroupId;

	private Action<uint> selectEvent = null;
	private bool isLock = false;

	public void Initialize()
	{
	}

	public void DoUpdate(GodLand_Table godLandTable, Action<uint> itemEvent, bool toggleOn)
	{
		gameObject.SetActive(true);

		godLandGroupId = godLandTable.SlotGroupID;

		selectEvent = itemEvent;

		isLock = Me.CurCharData.LastLevel < godLandTable.LevelLimit;
		GameObject activeObj = (isLock) ? mapPointLockObj : mapPointObj;
		GameObject inActiveObj = (isLock) ? mapPointObj : mapPointLockObj;
		activeObj.SetActive(true);
		inActiveObj.SetActive(false);

		// 지역이름
		foreach (var txt in activeObj.FindChildrenComponents<ZText>("Txt")) {
			txt.text = DBLocale.GetText(godLandTable.GodLandUpperTextID);
		}

		if (isLock) {
			// 레벨표시
			string strLv = string.Format(DBLocale.GetText("GodLand_LevelLimit"), godLandTable.LevelLimit);
			levelText1.text = strLv;
			levelText2.text = strLv;
		}

		// 재화텍스트
		foreach (var txt in activeObj.FindChildrenComponents<ZText>("Txt_Value")) {
			float amount = ((float)3600 / godLandTable.ProductionTime) * godLandTable.ProductionItemCount;
			txt.text = string.Format("{0:0}/H", amount);
		}

		// 재화아이콘
		foreach (var image in activeObj.FindChildrenComponents<ZImage>("Icon_Coin")) {
			var itemTable = DBItem.GetItem(godLandTable.ProductionItemID);
			image.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
		}

		if (godLandTable.LocalMapPosition.Count == 2) {
			this.transform.localPosition = new Vector3(godLandTable.WorldMapPosition[0], godLandTable.WorldMapPosition[1], 0);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"GodLandTable 로컬포지션오류, id:{godLandTable.GodLandID}/count:{godLandTable.WorldMapPosition.Count}");
		}
	}

	public void UIListItemClick()
	{
		if (isLock) {
			UIMessagePopup.ShowPopupOk(DBLocale.GetText("Message_LimitLevel"));
			return;
		}

		selectEvent?.Invoke(godLandGroupId);
	}
}


