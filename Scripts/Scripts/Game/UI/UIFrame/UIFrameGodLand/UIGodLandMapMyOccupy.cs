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

public class UIGodLandMapMyOccupy : MonoBehaviour
{
	[SerializeField] private ZImage classIcon;
	[SerializeField] private ZImage attributeIcon;
	[SerializeField] private ZImage charIcon;
	[SerializeField] private ZImage gradeBG;
	[SerializeField] private ZScrollBar percentGuage;
	[SerializeField] private ZText percentText;
	[SerializeField] private ZText stateText;
	[SerializeField] private ZButton quickMoveButton;
	[SerializeField] private GameObject redDotObj;

	private uint godLandTid;
	private Action<uint> clickEvent = null;
	private bool isHave;
	public void Initialize(Action<uint> _clickEvent)
	{
		clickEvent = _clickEvent;

		gameObject.SetActive(false);
	}

	public void Show(bool _isHave, GodLandSpotInfoConverted myData)
	{
		gameObject.SetActive(true);

		isHave = _isHave;

		if (_isHave) {
			ShowMyOccupy(myData);
		}
		else {
			EmptyMyOccupy(myData);
		}

		quickMoveButton.interactable = _isHave;
	}

	private void SetMyCharacterSlot(GodLandSpotInfoConverted data)
	{
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
	}

	private void EmptyMyOccupy(GodLandSpotInfoConverted data)
	{
		stateText.text = DBLocale.GetText("GodLand_Not_Exist_Mine");

		SetMyCharacterSlot(data);

		percentGuage.size = 0;
		percentText.text = string.Empty;

		redDotObj.SetActive(false);
	}

	private void ShowMyOccupy(GodLandSpotInfoConverted data)
	{
		godLandTid = data.GodLandTable.GodLandID;

		stateText.text = DBLocale.GetText("GodLand_Possession");

		if (data.UnitType == E_UnitType.Character) {
			SetMyCharacterSlot(data);

			//gauge
			float productionRate = (float)data.ProductionCnt / data.GodLandTable.ProductionItemCountMax;
			percentGuage.size = productionRate;
			percentText.text = string.Format("{0:0.0}%", productionRate * 100);

			redDotObj.SetActive(productionRate == 1);
		}
		else {
			ZLog.LogError(ZLogChannel.Default, $"알수없는 유닛타입, id:{data.UnitType}");
		}
	}

	public void OnClickQuickMove()
	{
		clickEvent?.Invoke(godLandTid);
	}

	public bool HasOccupy()
	{
		return isHave;
	}

	public bool IsActiveRedDot()
	{
		return redDotObj.activeSelf;
	}
}


