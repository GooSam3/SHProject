using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MonoAbilitySlotBase : MonoBehaviour
{
	public abstract void SetSlot(UIAbilityData data);
}

public class UIAbilitySlot : MonoAbilitySlotBase
{
	[SerializeField] private Text txtLeft;
	[SerializeField] private Text txtRight;
	[SerializeField] private GameObject objCompare;

	[SerializeField] private GameObject objUp;
	[SerializeField] private GameObject objDown;

	public override void SetSlot(UIAbilityData data)
	{
		SetSlotEmpty();


		switch (data.viewType)
		{
			case E_UIAbilityViewType.Blank:
				return;
			case E_UIAbilityViewType.Text:
				txtLeft.text = data.textLeft;
				txtRight.text = data.textRight;
				return;
			case E_UIAbilityViewType.Ability:
				if (data.useBySkillName)
				{
					SetSlotAbilityName(data);
					return;
				}
				else
				{
					string ability = DBLocale.GetText(DBAbility.GetAbilityName(data.type));

					txtLeft.text = ability;
					txtRight.text = DBAbility.ParseAbilityValue((uint)(data.type), data.value);
				}
				return;
			case E_UIAbilityViewType.AbilityCompare:
				{
					bool isCompare = data.value != data.compareValue;

					objCompare.SetActive(isCompare);

					string ability = DBLocale.GetText(DBAbility.GetAbilityName(data.type));

					txtLeft.text = ability;
					txtRight.text = DBAbility.ParseAbilityValue((uint)(data.type), data.value);

					if (isCompare)
					{
						bool isUp = data.value > data.compareValue;

						objUp.SetActive(isUp);
						objDown.SetActive(!isUp);

						bool isPositive = (data.value - data.compareValue) > 0;

						txtRight.text += UICommon.GetColoredText(isPositive ? "#51CFFF" : "#C93C5E", $"({(isPositive ? "+" : "")} {data.value - data.compareValue})");
					}
				}
				return;
		}
	}

	// 빈칸용~~
	public void SetSlotEmpty()
	{
		if (objCompare)
			objCompare.gameObject.SetActive(false);
		txtLeft.text = string.Empty;
		txtRight.text = string.Empty;

	}

	// 스킬이름 출력용~
	public void SetSlotAbilityName(UIAbilityData data)
	{
		SetSlotEmpty();

		if (DBSkill.TryGet(data.skillTarget, out var table) == false)
			return;

		txtLeft.text = DBUIResouce.GetSkillGradeFormat(
					   DBLocale.GetText("Ability_Slot_AbilityName", DBLocale.GetText(table.SkillTextID), DBLocale.GetCharacterTypeName(table.CharacterType)),
					   table.Grade);
	}

}
