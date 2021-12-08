using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupCollectionFilterSelect : UIPopupBase
{
	[Serializable]
	public class CCollectionFilter
	{
		public GameObject obj;
		public Text txt;
		public E_AbilityType type;
		public ZToggle toggle;
	}

	[SerializeField]
	private List<CCollectionFilter> listFilter = new List<CCollectionFilter>();

	[SerializeField] private GameObject objReset;	// 리셋버튼, 멀티전용
	[SerializeField] private GameObject objConfirm;	// 확인버튼, 멀티전용

	private Action<E_AbilityType> onClick;

	private Action<List<E_AbilityType>> onClickMultiple;

	bool isInitilized = false;

	bool isMultipleMode = false;

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		if (!isInitilized)
		{
			Init();
		}
	}

	public void SetListener(Action<E_AbilityType> _onClick)
	{
		onClick = _onClick;

		isMultipleMode = false;
		SetUI();
	}

	public void SetPopup(List<E_AbilityType> pastList, Action<List<E_AbilityType>> _onClickMultiple)
	{
		onClickMultiple = _onClickMultiple;

		isMultipleMode = true;
		SetUI();
		OnClickReset(pastList);
	}

	private void SetUI()
	{
		objReset.SetActive(isMultipleMode);
		objConfirm.SetActive(isMultipleMode);
	}

	private void Init()
	{
		foreach (var iter in listFilter)
		{
			iter.txt.text = DBLocale.GetText(iter.type.ToString());
		}
	}

	private void OnClickSlot(int dataIndex)
	{
		if (isMultipleMode)
			return;

		if (dataIndex >= listFilter.Count)
			return;

		onClick?.Invoke(listFilter[dataIndex].type);
		OnClickClose();
	}

	public void OnClickReset(List<E_AbilityType> listOnAbility)
	{
		foreach(var iter in listFilter)
		{
			bool isOn = listOnAbility.Contains(iter.type);

			iter.toggle.SelectToggleSingle(isOn, false);
		}
	}

	public void OnClickMultiConfrim()
	{
		List<E_AbilityType> listAbility = new List<E_AbilityType>();

		foreach(var iter in listFilter)
		{
			if (iter.toggle.isOn)
				listAbility.Add(iter.type);
		}

		onClickMultiple?.Invoke(listAbility);
		OnClickClose();
	}

	public void OnClickClose()
	{
		UIManager.Instance.Close<UIPopupCollectionFilterSelect>(true);
	}

#if UNITY_EDITOR
	[ContextMenu("RESET__LISTITEM")]
	public void ResetListItems()
	{
		string[] abilities ={"STR_PLUS"
							,"DEX_PLUS"
							,"INT_PLUS"
							,"VIT_PLUS"
							,"WIS_PLUS"
							,"MAX_HP_PLUS"
							,"MAX_MP_PLUS"
							,"SHORT_ATTACK_PLUS"
							,"LONG_ATTACK_PLUS"
							,"MAGIC_ATTACK_PLUS"
							,"SHORT_ACCURACY_PLUS"
							,"LONG_ACCURACY_PLUS"
							,"MAGIC_ACCURACY_PLUS"
							,"SHORT_CRITICAL_PLUS"
							,"LONG_CRITICAL_PLUS"
							,"MAGIC_CRITICAL_PLUS"
							,"SHORT_CRITICAL_MINUS"
							,"LONG_CRITICAL_MINUS"
							,"MAGIC_CRITICAL_MINUS"
							,"SHORT_CRITICALDAMAGE_PLUS"
							,"LONG_CRITICALDAMAGE_PLUS"
							,"MAGIC_CRITICALDAMAGE_PLUS"
							,"SHORT_CRITICALDAMAGE_MINUS"
							,"LONG_CRITICALDAMAGE_MINUS"
							,"MAGIC_CRITICALDAMAGE_MINUS"
							,"MELEE_DEFENCE_PLUS"
							,"MAGIC_DEFENCE_PLUS"
							,"MOVE_SPEED_PER"
							,"ATTACK_SPEED_PER"
							,"REDUCTION_PLUS"
							,"REDUCTION_IGNORE_PLUS"
							,"SHORT_EVASION_PLUS"
							,"LONG_EVASION_PLUS"
							,"MAGIC_EVASION_PLUS"
							,"SHORT_EVASION_IGNORE_PLUS"
							,"LONG_EVASION_IGNORE_PLUS"
							,"MAGIC_EVASION_IGNORE_PLUS"
							,"MAX_WEIGH_PLUS"
							,"HP_AUTO_RECOVERY_PLUS"
							,"MP_AUTO_RECOVERY_PLUS"
							,"POTION_RECOVERY_PLUS"
							,"POTION_RECOVERY_PER"
							,"POTION_RECOVERY_TIME_PER"
							,"MAZ_RATE_UP_PER"
							,"MAZ_RATE_DOWN_PER"};

		var children = GetComponentsInChildren<Transform>();

		int idx = 0;
		foreach (var iter in children)
		{
			if (iter.name.Contains("Bt_Radio_Txt") == false)
				continue;

			if (idx >= abilities.Length)
			{
				iter.gameObject.SetActive(false);
				continue;
			}

			if (Enum.TryParse<E_AbilityType>(abilities[idx], out E_AbilityType type) == false)
			{
				idx++;
				continue;
			}

			var filter = new CCollectionFilter();

			filter.type = type;
			filter.obj = iter.gameObject;
			filter.txt = iter.GetComponentInChildren<ZText>();


			var tog = iter.GetComponent<ZToggle>();
			UnityEditor.Events.UnityEventTools.AddIntPersistentListener(tog.onValueChanged, OnClickSlot, idx);

			filter.toggle = tog;

			listFilter.Add(filter);

			idx++;
		}
	}

#endif
}
