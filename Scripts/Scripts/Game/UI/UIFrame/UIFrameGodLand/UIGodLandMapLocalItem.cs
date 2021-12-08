using DG.Tweening;
using NTSDK.Module.Chatus.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using uTools;
using GameDB;
using ZNet.Data;

public class UIGodLandMapLocalItem : MonoBehaviour
{
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZImage attributeIcon;
	[SerializeField] private ZImage charIcon;
	[SerializeField] private ZImage gradeBG;
	[SerializeField] private ZScrollBar percentGuage;
	[SerializeField] private ZText percentText;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZText nameText;
	[SerializeField] private GameObject myMark;
	[SerializeField] private GameObject selectLine;
	[SerializeField] private GameObject percentGroup;
	[SerializeField] private GameObject redDotObj;

	private uint godLandTid;
	private Action<uint> clickEvent = null;

	private CanvasGroup canversGroup;
	private uTweenAlpha tweenAlpha;
	private uTweenPosition tweenPos;

	public void Initialize()
	{
		canversGroup = gameObject.GetComponent<CanvasGroup>();
		tweenAlpha = gameObject.GetComponent<uTweenAlpha>();
		tweenPos = gameObject.GetComponentInChildren<uTweenPosition>();
	}

	//public void SetFocus(uint selectGodLandTid)
	//{
	//	selectLine.SetActive(godLandTid == selectGodLandTid);
	//}

	//public void ClearFocus()
	//{
	//	selectLine.SetActive(false);
	//}

	public void DoUpdate(GodLandSpotInfoConverted data, bool fromServer, Action<uint> _clickEvent)
	{
		gameObject.SetActive(true);

		canversGroup.alpha = 0;
		tweenAlpha.enabled = false;
		tweenPos.enabled = false;

		godLandTid = data.GodLandTable.GodLandID;

		//selectLine.SetActive(data.IsSelected);

		//버튼위치
		var godLandTable = data.GodLandTable;
		if (godLandTable.LocalMapPosition.Count == 2) {
			this.transform.localPosition = new Vector3(godLandTable.LocalMapPosition[0], godLandTable.LocalMapPosition[1], 0);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"GodLandTable 로컬포지션오류, id:{data.GodLandTable.GodLandID}/count:{godLandTable.LocalMapPosition.Count}");
		}

		if (fromServer == false) {
			clickEvent = null;
			return;
		}

		clickEvent = _clickEvent;

		var delay = UnityEngine.Random.Range(0, 0.3f);
		tweenAlpha.ResetToBeginning();
		tweenAlpha.delay = delay;
		tweenAlpha.enabled = true;
		tweenPos.ResetToBeginning();
		tweenPos.enabled = true;
		tweenPos.delay = delay;

		bool bCharacter = data.UnitType == E_UnitType.Character;
		classIcon.gameObject.SetActive(bCharacter);
		percentGroup.SetActive(bCharacter);

		//my mark
		myMark.SetActive(data.TargetType == E_TargetType.Self);

		if (bCharacter) {
			var characterTable = DBCharacter.Get(data.CharTid);
			if (characterTable == null) {
				ZLog.LogError(ZLogChannel.Default, $"DBCharacter 가 null이다, CharTid:{data.CharTid }");
				return;
			}

			if (data.ChangeTid == 0) {
				//character
				charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.CharacterIcon);
				gradeBG.sprite = UICommon.GetGradeSprite(1);
				classIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.Icon);
				//attribute
				attributeIcon.sprite = UICommon.GetAttributeSprite(characterTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
			}
			else {
				//character
				var changeTable = DBChange.Get(data.ChangeTid);
				charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(changeTable.Icon);
				gradeBG.sprite = UICommon.GetGradeSprite(changeTable.Grade);
				classIcon.sprite = UICommon.GetClassIconSprite(changeTable.ClassIcon, UICommon.E_SIZE_OPTION.Small);
				//attribute
				attributeIcon.sprite = UICommon.GetAttributeSprite(changeTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
			}

			//name & level
			nameText.text = data.Nick;
			levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), data.Lv);

			//gauge
			float productionRate = (float)data.ProductionCnt / data.GodLandTable.ProductionItemCountMax;
			percentGuage.size = productionRate;
			percentText.text = string.Format("{0:0.0}%", productionRate * 100);

			redDotObj.SetActive(data.TargetType == E_TargetType.Self && productionRate == 1);
		}
		else if (data.UnitType == E_UnitType.Monster) {
			//character
			charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(godLandTable.Icon);
			gradeBG.sprite = UICommon.GetGradeSprite(1);

			//name & level
			var monsterTable = DBMonster.Get(data.MonsterTid);
			nameText.text = DBLocale.GetText(monsterTable.MonsterTextID);
			levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), monsterTable.Level);
			//attribute
			attributeIcon.sprite = UICommon.GetAttributeSprite(monsterTable.AttributeType, UICommon.E_SIZE_OPTION.Small);

			redDotObj.SetActive(false);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"알수없는 유닛타입, id:{data.UnitType}");
		}
	}

	public void OnClickListItem()
	{
		ZLog.Log(ZLogChannel.Default, $"OnClickListItem, godLandTid:{godLandTid}");

		clickEvent?.Invoke(godLandTid);
	}
}


