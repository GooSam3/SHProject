using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

public class UIPopupGodLandResultItem : MonoBehaviour
{
	[SerializeField] private ZText titleText;
	[SerializeField] private ZText nickName;
	[SerializeField] private ZText levelText;
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZImage attributeIcon;
	[SerializeField] private ZImage charIcon;
	[SerializeField] private ZImage gradeBG;
	[SerializeField] private ZText desc1;
	[SerializeField] private ZText desc2;
	[SerializeField] private ZImage icon1;
	[SerializeField] private ZImage icon2;

	// 획득팝업
	public void ShowCompletePopup(uint getItemTid, ulong getCnt, uint timeCnt)
	{
		gameObject.SetActive(true);

		titleText.text = DBLocale.GetText("GodLand_Popup_Gather_Complete");

		string title = DBLocale.GetText("GodLand_Popup_Gather_Total_Time");
		string timeText = TimeHelper.GetRemainFullTimeMin(timeCnt);
		desc1.text = $"{title} : {timeText}";

		string amountTile = DBLocale.GetText("GodLand_Popup_Gather_Total_Item");
		desc2.text = $"{amountTile} : {getCnt}";

		//icon1
		var itemTable = DBItem.GetItem(getItemTid);
		icon2.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
	}

	// 전투결과 팝업
	public void ShowResultPopup(uint godLandTid, bool isWin)
	{
		gameObject.SetActive(true);

		var godLandTable = DBGodLand.Get(godLandTid);

		if (isWin) {
			titleText.text = DBLocale.GetText("GodLand_Robbery_Success_Title");

			string amountTile = DBLocale.GetText("GodLand_Time_Production"); //시간당 생산량
			float amount = ((float)3600 / godLandTable.ProductionTime) * godLandTable.ProductionItemCount;
			desc1.text = string.Format("{0}:{1:0}", amountTile, amount);

			amountTile = DBLocale.GetText("GodLand_Max_Production"); //최대 생산량
			desc2.text = $"{amountTile}:{godLandTable.ProductionItemCountMax}";

			var itemTable = DBItem.GetItem(godLandTable.ProductionItemID);
			icon1.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
			icon2.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
		}
		else {
			titleText.text = DBLocale.GetText("GodLand_Robbery_Failure_Title");

			desc1.text = DBLocale.GetText("GodLand_Robbery_Failure");
		}

		nickName.text = Me.CurCharData.Nickname;
		levelText.text = string.Format(DBLocale.GetText("GodLand_CharacterLV"), Me.CurCharData.Level);

		var characterTable = DBCharacter.Get(Me.CurCharData.TID);
		if (characterTable == null) {
			ZLog.LogError(ZLogChannel.Default, $"DBCharacter 가 null이다, CharTid:{Me.CurCharData.TID}");
			return;
		}

		if (Me.CurCharData.CurrentMainChange == 0) {
			//character
			charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.CharacterIcon);
			gradeBG.sprite = UICommon.GetGradeSprite(1);
			classIcon.sprite = ZManagerUIPreset.Instance.GetSprite(characterTable.Icon);
			//attribute
			attributeIcon.sprite = UICommon.GetAttributeSprite(characterTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
		}
		else {
			//character
			var changeTable = DBChange.Get(Me.CurCharData.MainChange);
			charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(changeTable.Icon);
			gradeBG.sprite = UICommon.GetGradeSprite(changeTable.Grade);
			classIcon.sprite = UICommon.GetClassIconSprite(changeTable.ClassIcon, UICommon.E_SIZE_OPTION.Small);
			//attribute
			attributeIcon.sprite = UICommon.GetAttributeSprite(changeTable.AttributeType, UICommon.E_SIZE_OPTION.Small);
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);


	}
}
