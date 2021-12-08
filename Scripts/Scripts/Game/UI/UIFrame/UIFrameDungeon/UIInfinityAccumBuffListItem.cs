using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfinityAccumBuffListItem : MonoBehaviour
{
	[SerializeField] private ZImage BuffIcon;
	[SerializeField] private ZText BuffName;
	[SerializeField] private ZText BuffOptions;

	public void Init(AbilityAction_Table abilityAction)
	{
		BuffName.text = DBLocale.GetText(abilityAction.NameText);
		BuffIcon.sprite = ZManagerUIPreset.Instance.GetSprite(abilityAction.BuffIconString);
		
		string options = string.Empty;
		Dictionary<E_AbilityType, ValueTuple<float, float>> abilitys = new Dictionary<E_AbilityType, ValueTuple<float, float>>();

		uint abilityActionId = abilityAction.AbilityID_01 == E_AbilityType.LINK_ABILITY_BUFF ? abilityAction.LinkAbilityActionID : abilityAction.AbilityActionID;

		abilitys = DBAbility.GetAllAbilityData(abilityActionId);
		
		foreach(var ability in abilitys)
		{
			if(!DBAbility.IsParseAbility(ability.Key))
			{
				continue;
			}

			float abilityMinValue = (int)abilitys[ability.Key].Item1;
			float abilityMaxValue = (int)abilitys[ability.Key].Item2;

			options += DBLocale.GetText(DBAbility.GetAbilityName(ability.Key)) + DBAbility.ParseAbilityValue(ability.Key, abilityMinValue, abilityMaxValue) + "\n";
		}

		BuffOptions.text = options;
	}
}
