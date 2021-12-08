using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIGodLandLocalViewerItem : MonoBehaviour
{
	[SerializeField] private ZText localName;
	[SerializeField] private ZText nameText;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZImage productionImage;
	[SerializeField] private ZText productionCount;
	[SerializeField] private ZImage productionMaxImage;
	[SerializeField] private ZText productionMaxCount;
	[SerializeField] private GameObject selectLine;

	private uint godLandTid;
	private Action<uint> clickItem;

	public void SetData(GodLandSpotInfoConverted data, Action<uint> _clickItem)
	{
		selectLine.SetActive(data.IsSelected);

		godLandTid = data.GodLandTid;
		clickItem = _clickItem;

		//지역명
		localName.text = DBLocale.GetText(data.GodLandTable.GodLandTextID);

		bool bCharacter = data.UnitType == E_UnitType.Character;
		classIcon.enabled = bCharacter;

		if (data.UnitType == E_UnitType.Character) {
			//캐릭터
			if (data.ChangeTid == 0) {
				var characterTable = DBCharacter.Get(data.CharTid);
				if (characterTable == null) {
					ZLog.LogError(ZLogChannel.Default, $"DBCharacter 가 null이다, CharTid:{data.CharTid }");
					return;
				}
				classIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.Icon);
			}
			else {
				var changeTable = DBChange.Get(data.ChangeTid);
				classIcon.sprite = UICommon.GetClassIconSprite(changeTable.ClassIcon, UICommon.E_SIZE_OPTION.Small);
			}

			//닉네임 & 레벨표시
			nameText.text = data.Nick;
			levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), data.Lv);
		}
		else if (data.UnitType == E_UnitType.Monster) {
			//name & level
			var monsterTable = DBMonster.Get(data.MonsterTid);
			nameText.text = DBLocale.GetText(monsterTable.MonsterTextID);
			levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), monsterTable.Level);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"알수없는 유닛타입, id:{data.UnitType}");
		}

		//생산아이템 표시
		var itemTable = DBItem.GetItem(data.GodLandTable.ProductionItemID);
		if (itemTable != null) {
			productionImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
			productionMaxImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);

			float amount = ((float)3600 / data.GodLandTable.ProductionTime) * data.GodLandTable.ProductionItemCount;
			productionCount.text = string.Format("{0:0}/H", amount);
			productionMaxCount.text = $"{ data.GodLandTable.ProductionItemCountMax}";
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"해당재화에 해당하는 아이템을 찾을수 없다, itemTid:{data.GodLandTable.ProductionItemID}");
		}
	}

	public void OnClickItem()
	{
		clickItem?.Invoke(godLandTid);
	}
}