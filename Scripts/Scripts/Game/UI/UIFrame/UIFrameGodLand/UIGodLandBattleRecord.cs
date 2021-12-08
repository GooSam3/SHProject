using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using ZNet.Data;

public class UIGodLandBattleRecord : MonoBehaviour
{
	[SerializeField] private UIGodLandBattleRecordItemAdapter ScrollAdapter;
	[SerializeField] private UIGodLandBattleRecordItem recordItemPf;
	[SerializeField] private ZText noticeText;

	[SerializeField] private ZText mainTitle;
	[SerializeField] private ZText resultTitle;
	[SerializeField] private ZText enemyTitle;
	[SerializeField] private ZText guildNameTitle;

	[SerializeField] private GameObject redDot;

	public void Initialize()
	{
		mainTitle.text = DBLocale.GetText("GodLand_Popup_BattleLog");
		resultTitle.text = DBLocale.GetText("GodLand_Popup_BattleResult");
		guildNameTitle.text = DBLocale.GetText("GodLand_Popup_EnemyGuildName");
		enemyTitle.text = DBLocale.GetText("GodLand_Popup_BattleEnemy");

		ScrollAdapter.Parameters.ItemPrefab = recordItemPf.GetComponent<RectTransform>();
		var pf = ScrollAdapter.Parameters.ItemPrefab;
		pf.SetParent(recordItemPf.transform.parent);
		pf.localScale = Vector2.one;
		pf.localPosition = Vector3.zero;
		pf.gameObject.SetActive(false);
		ScrollAdapter.Initialize();
	}

	public void Show()
	{
		gameObject.SetActive(true);

		RefreshSelf();
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	private void RefreshSelf()
	{
		Me.CurCharData.GodLandContainer.REQ_GetGodLandFightRecord(true, (list) => {
			ScrollAdapter.SetNormalizedPosition(1);
			ScrollAdapter.Refresh(list);

			// 리스트가 없는경우 문구알림

			noticeText.enabled = list.Count == 0;
			if (list.Count == 0) {
				noticeText.text = DBLocale.GetText("GodLand_Popup_BattleLog_Not_Exist");
			}

			//bool bChanged = Me.CurCharData.GodLandContainer.IsBattleRecordChanged(list);

			Me.CurCharData.GodLandContainer.SaveBattleRecord(list);
			redDot.SetActive(false);
		});
	}

	public void OnClickClose()
	{
		Hide();
	}

	public bool IsActiveRedDot()
	{
		return redDot.activeSelf;
	}
}