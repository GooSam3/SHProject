using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UICookResultPopup : MonoBehaviour
{
    [SerializeField] private Text Name;
	[SerializeField] private Image Icon;
	[SerializeField] private Text Weight;
	[SerializeField] private Text Desc;
	[SerializeField] private Text[] Ability;

	public void Initialize(uint _itemTid)
	{
		if (_itemTid == 0)
			return;

		var table = DBItem.GetItem(_itemTid);

		if (table == null)
			return;

		gameObject.SetActive(true);

		Name.text = DBLocale.GetText(table.ItemTextID);
		Icon.sprite = UICommon.GetItemIconSprite(_itemTid);
		Weight.text = table.Weight.ToString();
		Desc.text = DBLocale.GetText(table.TooltipID);
		if (table.AbilityActionID_01 != 0) Ability[0].text = DBLocale.GetText(DBAbility.GetAbility(DBAbilityAction.Get(table.AbilityActionID_01).AbilityID_01).StringName); else Ability[0].text = "-";
		if (table.AbilityActionID_02 != 0) Ability[1].text = DBLocale.GetText(DBAbility.GetAbility(DBAbilityAction.Get(table.AbilityActionID_02).AbilityID_02).StringName); else Ability[1].text = "-";
		if (table.AbilityActionID_03 != 0) Ability[2].text = DBLocale.GetText(DBAbility.GetAbility(DBAbilityAction.Get(table.AbilityActionID_03).AbilityID_03).StringName); else Ability[2].text = "-";
	}
}