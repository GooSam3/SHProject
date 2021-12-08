
using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using ZNet.Data;
using ParadoxNotion;

public class UIGodLandLocalDetailEnemyItemSlot : MonoBehaviour
{
	[SerializeField] private ZImage charIcon;
	[SerializeField] private ZImage gradeBG;
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZImage attributeIcon;
	[SerializeField] private ZText nameText;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZText atkText;
	[SerializeField] private ZText defText;
	[SerializeField] private ZText mdefText;

	public void SetData(GodLandSpotInfoConverted data, bool isAtkHigh, bool isDefHigh, bool isMdefHigh)
	{
		atkText.text = $"{data.Atk}";
		defText.text = $"{data.Def}";
		mdefText.text = $"{data.Mdef}";

		atkText.color = isAtkHigh ? Color.white : ColorUtils.HexToColor("707480");
		defText.color = isDefHigh ? Color.white : ColorUtils.HexToColor("707480");
		mdefText.color = isMdefHigh ? Color.white : ColorUtils.HexToColor("707480");

		bool bCharacter = data.UnitType == E_UnitType.Character;
		classIcon.gameObject.SetActive(bCharacter);

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
		}
		else if (data.UnitType == E_UnitType.Monster) {
			//character
			charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.GodLandTable.Icon);
			gradeBG.sprite = UICommon.GetGradeSprite(1);

			//name & level
			var monsterTable = DBMonster.Get(data.MonsterTid);
			nameText.text = DBLocale.GetText(monsterTable.MonsterTextID);
			levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), monsterTable.Level);
			//attribute
			attributeIcon.sprite = UICommon.GetAttributeSprite(monsterTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"알수없는 유닛타입, id:{data.UnitType}");
		}
	}
}