using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySlotEnhancePR : MonoAbilitySlotBase
{
	[SerializeField] private Text txtAbilityName;

	[SerializeField] private Text txtAbilityBefore;
	[SerializeField] private Text txtAbilityAfter;

	[SerializeField] private Text txtChanged;

	[SerializeField] private GameObject objUp;

	[SerializeField] private GameObject objStatGroup;

	[SerializeField] private Text txtNotice;

	public override void SetSlot(UIAbilityData data)
	{
		SetDefault();

		switch (data.viewType)
		{
			case E_UIAbilityViewType.Text:
				txtNotice.text = data.textLeft;
				return;
			case E_UIAbilityViewType.AbilityCompare:
				if (objStatGroup)
					objStatGroup.SetActive(true);
				break;
			default:
				return;
		}

		txtAbilityName.text = DBLocale.GetText(DBAbility.GetAbilityName(data.type));
		txtAbilityBefore.text = DBAbility.ParseAbilityValue((uint)data.type, data.compareValue);
		txtAbilityAfter.text = DBAbility.ParseAbilityValue((uint)data.type, data.value);

		float compareValue = data.value - data.compareValue;
		bool hasChange = compareValue > float.Epsilon;

		objUp.SetActive(hasChange);

		if (hasChange)
		{
			txtChanged.text = UICommon.GetColoredText("#51CFFF", $"(+{compareValue})");
		}
	}

	public void SetDefault()
	{
		if(objStatGroup)
			objStatGroup.SetActive(false);

		txtAbilityName.text = string.Empty;
		txtAbilityBefore.text = string.Empty;
		txtAbilityAfter.text = string.Empty;
		txtChanged.text = string.Empty;
		objUp.SetActive(false);

		if (txtNotice)
			txtNotice.text = string.Empty;
	}
}
