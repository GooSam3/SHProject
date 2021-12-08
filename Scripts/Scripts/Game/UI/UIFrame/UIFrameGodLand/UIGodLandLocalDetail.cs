using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using ZNet.Data;
using uTools;

public class UIGodLandLocalDetail : MonoBehaviour
{
	[SerializeField] private UIGodLandLocalDetailMyItem uiDetailMyItem;
	[SerializeField] private UIGodLandLocalDetailEnemyItem uiDetailEnemyItem;
	[SerializeField] private ZText localName;
	[SerializeField] private ZText stateText;
	[SerializeField] private uTweenAlpha tweenAlpha;

	private Action closeCallback;
	private uint ownerGodLandTid;

	public void Initialize(Action<E_TargetType, uint> _actionCallback, Action _closeCallback)
	{
		closeCallback = _closeCallback;

		uiDetailMyItem.Initialize(_actionCallback);
		uiDetailEnemyItem.Initialize(_actionCallback);
	}

	public void Show(uint godLandTid, bool isHave, GodLandSpotInfoConverted myData)
	{
		if (isHave && myData.GodLandTid == godLandTid) {
			// 현재 상세보기하려는 곳이 내꺼다!
			RefreshUI(myData);

			uiDetailEnemyItem.Hide();
			uiDetailMyItem.Show(myData);
		}
		else {
			// 내께 없거나 내께 아닐 경우
			var ownerData = Me.CurCharData.GodLandContainer.SpotInfoList.Find(v => v.GodLandTid == godLandTid);
			if (ownerData == null) {
				ZLog.LogError(ZLogChannel.Default, $"성지 상세뷰에서 데이타가 null 이다");
				return;
			}
			if (ownerData.TargetType != E_TargetType.Enemmy) {
				ZLog.LogError(ZLogChannel.Default, $"성지 상세뷰에서 타겟이 적이 아니다 {ownerData.TargetType}");
				return;
			}

			RefreshUI(ownerData);

			uiDetailMyItem.Hide();
			uiDetailEnemyItem.Show(ownerData, myData);

			// 초상화 fade in 효과
			if (ownerGodLandTid != ownerData.GodLandTid) {
				ownerGodLandTid = ownerData.GodLandTid;
				tweenAlpha.ResetToBeginning();
				tweenAlpha.enabled = true;
			}
		}
	}

	private void RefreshUI(GodLandSpotInfoConverted ownerData)
	{
		gameObject.SetActive(true);
		localName.text = DBLocale.GetText(ownerData.GodLandTable.GodLandTextID);

		if (ownerData.TargetType == E_TargetType.Self) {
			stateText.text = DBLocale.GetText("GodLand_Possession");
		}
		else {
			//if (ownerData.UnitType == E_UnitType.Character) {
			//	stateText.text = DBLocale.GetText("GodLand_Possible_Robbery");
			//}
			//else {
			//	stateText.text = DBLocale.GetText("GodLand_Possible_Possession");
			//}
			stateText.text = DBLocale.GetText("GodLand_Possible_Robbery");
		}
	}

	public void Hide()
	{
		uiDetailMyItem.Hide();
		uiDetailEnemyItem.Hide();

		gameObject.SetActive(false);
	}

	public void OnClickClose()
	{
		closeCallback?.Invoke();
	}
}