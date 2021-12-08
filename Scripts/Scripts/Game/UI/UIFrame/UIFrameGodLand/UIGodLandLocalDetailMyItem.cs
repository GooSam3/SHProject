using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using ZNet.Data;

public class UIGodLandLocalDetailMyItem : MonoBehaviour
{
	[SerializeField] private ZImage charIcon;
	[SerializeField] private ZImage attributeIcon;
	[SerializeField] private ZImage gradeBG;
	[SerializeField] private ZText nameText;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZScrollBar percentGuage;
	[SerializeField] private ZText percentText;
	[SerializeField] private ZText productionName;
	[SerializeField] private ZText produnctionCurName;
	[SerializeField] private ZText actionButtonName;
	[SerializeField] private ZText productionAvgCount;
	[SerializeField] private ZImage productionAvgImage;
	[SerializeField] private ZText productionMaxCount;
	[SerializeField] private ZImage productionMaxImage;
	[SerializeField] private ZText produnctionCurCount;
	[SerializeField] private ZImage produnctionCurImage;
	[SerializeField] private ZButton actionButton;
	[SerializeField] private GameObject redDotObj;

	private Action<E_TargetType, uint> actionCallback;

	private uint godLandTid;

	public void Initialize(Action<E_TargetType, uint> _actionCallback)
	{
		productionName.text = DBLocale.GetText("GodLand_Production_Info");
		produnctionCurName.text = DBLocale.GetText("GodLand_Current_Production");
		actionButtonName.text = DBLocale.GetText("GodLand_Get_Button");

		actionCallback = _actionCallback;
	}

	public void Show(GodLandSpotInfoConverted _myData)
	{
		gameObject.SetActive(true);

		godLandTid = _myData.GodLandTid;

		//캐릭터
		if (_myData.ChangeTid == 0) {
			var characterTable = DBCharacter.Get(_myData.CharTid);
			if (characterTable == null) {
				ZLog.LogError(ZLogChannel.Default, $"DBCharacter 가 null이다, CharTid:{_myData.CharTid }");
				return;
			}
			charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.CharacterIcon);
			gradeBG.sprite = UICommon.GetGradeSprite(1);
			classIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.Icon);
			//attribute
			attributeIcon.sprite = UICommon.GetAttributeSprite(characterTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
		}
		else {
			var changeTable = DBChange.Get(_myData.ChangeTid);
			charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(changeTable.Icon);
			gradeBG.sprite = UICommon.GetGradeSprite(changeTable.Grade);
			classIcon.sprite = UICommon.GetClassIconSprite(changeTable.ClassIcon, UICommon.E_SIZE_OPTION.Small);
			//attribute
			attributeIcon.sprite = UICommon.GetAttributeSprite(changeTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
		}

		//닉네임 & 레벨표시
		nameText.text = _myData.Nick;
		levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), _myData.Lv);

		//생산아이템 표시
		var itemTable = DBItem.GetItem(_myData.GodLandTable.ProductionItemID);
		if (itemTable != null) {
			productionAvgImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
			productionMaxImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
			produnctionCurImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);

			string amountTile = DBLocale.GetText("GodLand_Time_Production"); //시간당 생산량
			float amount = ((float)3600 / _myData.GodLandTable.ProductionTime) * _myData.GodLandTable.ProductionItemCount;
			productionAvgCount.text = string.Format("{0}:{1:0}", amountTile, amount);

			amountTile = DBLocale.GetText("GodLand_Max_Production"); //최대 생산량
			productionMaxCount.text = $"{amountTile}:{ _myData.GodLandTable.ProductionItemCountMax}";

			// 현재생산량
			produnctionCurCount.text = $"{_myData.ProductionCnt}";

			//gauge
			float productionRate = (float)_myData.ProductionCnt / _myData.GodLandTable.ProductionItemCountMax;
			percentGuage.size = productionRate;

			// 생산시간과 생산백분율 표시
			ShowTimeText(_myData.TimeCnt, productionRate);

			actionButton.interactable = _myData.ProductionCnt > 0;

			redDotObj.SetActive(productionRate == 1);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"해당재화에 해당하는 아이템을 찾을수 없다, itemTid:{_myData.GodLandTable.ProductionItemID}");
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void OnClickAction()
	{
		actionCallback?.Invoke(E_TargetType.Self, godLandTid);
	}

	//실시간이라면
	//private float secondCheckTimer;
	//private void Update()
	//{
	//	UpdateTimer();
	//}
	//private void UpdateTimer()
	//{
	//	secondCheckTimer += Time.deltaTime;
	//	if (secondCheckTimer >= 0.95f) {
	//		secondCheckTimer = 0;
	//		ulong sumTime = (TimeManager.NowSec - myData.receivedPacketTime) + myData.TimeCnt;
	//		float productionValue = (float)myData.ProductionCnt / myData.GodLandTable.ProductionItemCountMax;
	//		ShowTimeText(sumTime, productionValue);
	//	}
	//}

	private void ShowTimeText(ulong time, float productionValue)
	{
		string timeText = TimeHelper.GetRemainFullTimeMin(time);
		string curPercent = string.Format("{0:0.0}%", productionValue * 100);
		percentText.text = $"{timeText} ({curPercent})";
	}
}