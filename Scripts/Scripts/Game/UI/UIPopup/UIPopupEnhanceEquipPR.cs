using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIPopupEnhanceEquipPR : CUIFramePopupBase
{
	// 강화 아이템 및 능력치
	[SerializeField] private UIPREquipListItem enhanceTarget;
	[SerializeField] private UIAbilityListAdapter abilityAdapter;

	// 강화할 아이템 이름
	[SerializeField] private Text txtEnhanceTarget;

	// 강화 수치
	[SerializeField] private Text txtEnhanceStepBefore;
	[SerializeField] private Text txtEnhanceStepAfter;

	// 강화 능력치
	[SerializeField] private UIAbilitySlotEnhancePR slotMainAbility;
	[SerializeField] private UIAbilitySlotEnhancePR slotFirstAbility;

	// 세트정보
	[SerializeField] private Text txtSetInfo;

	// 연속강화 토글구ㅡ룹
	[SerializeField] private ZToggleGroup toggleGroupAutoEnhance;

	[SerializeField] private ZToggle toggleAutoEnhance;
	
	// 강화버튼
	[SerializeField] private ZButton btnEnhance;

	[SerializeField] private ZButton btnStopAutoEnhance;
	// 소모재화
	[SerializeField] private Text txtCost;

	// -- 연출관련 -- 
	[SerializeField] private FxInteractableGroup fxInteractable;
	[SerializeField] private UIFxParticle fxStart;
	[SerializeField] private UIFxParticle fxSuccess;
	[SerializeField] private UIFxParticle fxFail;
	// -------------

	// -- osa관련 갱신용 -- 
	[SerializeField] private ContentSizeFitter fitter;
	[SerializeField] private VerticalLayoutGroup layoutGroup;
	// -------------------

	// 현재 강화장비
	private PetRuneData runeData;

	// 강화 가격
	private ulong cost;

	// 닫았을시 액션
	private Action onClose;

	private bool isAutoEnhance = false;
	private int autoEnhanceTargetStep = 0;
	private bool isAutoEnhancePlaying = false;

	private RuneEnchant_Table tableBaseEnchant;
	private RuneEnchant_Table tableNextEnchant;

	protected override void OnInitialize()
	{
		base.OnInitialize();

		abilityAdapter.Initialize();
		abilityAdapter.enabled = false;

		layoutGroup.enabled = true;
		fitter.enabled = true;

		toggleGroupAutoEnhance.gameObject.SetActive(false);

		isAutoEnhance = false;
		autoEnhanceTargetStep = 0;

		ResetFx(true);
	}

	public void SetEnhanceEquip(PetRuneData data, Action _onClose)
	{
		runeData = data;
		enhanceTarget.SetSlot(data);
		onClose = _onClose;

		tableBaseEnchant = DBRune.GetRuneEnchantTable(data.BaseEnchantTid);
		tableNextEnchant = DBRune.GetNextRuneEnchantTable(data.BaseEnchantTid, tableBaseEnchant.GroupID);

		txtEnhanceTarget.text = $"{DBLocale.GetText(DBItem.GetItem(data.RuneTid).ItemTextID)}{UICommon.GetEnchantText(tableBaseEnchant.EnchantStep)}";

		RefreshCost(tableNextEnchant);

		txtEnhanceStepBefore.text = $"+{tableBaseEnchant.EnchantStep}";

		var mainAbility = DBRune.GetMainAbility(tableBaseEnchant);
		var firstAbility = DBRune.GetFirstAbility(data);

		var subAbility = DBRune.GetSubAbility(data);

		// 최대강화상태..!
		if (tableNextEnchant == null)
		{
			txtEnhanceStepAfter.text = $"+{DBLocale.GetText("Attribute_Enhance_MaxButtont")}";
			//subAbility.Add(new UIAbilityData(E_UIAbilityViewType.Blank));
			subAbility.Add(new UIAbilityData(E_UIAbilityViewType.Text) { textLeft = DBLocale.GetText("Mark_Enchant_Max") });
		}
		else
		{
			txtEnhanceStepAfter.text = $"+{tableNextEnchant.EnchantStep}";

			UIStatHelper.SetPetEquipCompareEnhanceStat(ref mainAbility, ref firstAbility, ref subAbility, tableNextEnchant);
			//부옵션 변경
			if (tableNextEnchant.GetSupOptionType == GameDB.E_GetSupOptionType.GetSupOption)
			{
				// subAbility.Add(new UIAbilityData(E_UIAbilityViewType.Blank));

				bool isAddSubOption = data.GetSubOptionCount() < tableNextEnchant.SupOptionCount;

				// 부옵션 부여
				if (tableBaseEnchant.SupOptionCount < tableNextEnchant.SupOptionCount)
				{
					subAbility.Add(new UIAbilityData(E_UIAbilityViewType.Text) { textLeft = DBLocale.GetText("Sub_Option_Grant_Text") });
				}
				else// 부 옵션 강화
				{
					subAbility.Add(new UIAbilityData(E_UIAbilityViewType.Text) { textLeft = DBLocale.GetText("Sub_Option_Grant_Text") });
				}
			}
		}

		slotMainAbility.gameObject.SetActive(mainAbility != null);
		if (mainAbility != null)
			slotMainAbility.SetSlot(mainAbility);

		slotFirstAbility.gameObject.SetActive(firstAbility != null);
		if (firstAbility != null)
			slotFirstAbility.SetSlot(firstAbility);

		abilityAdapter.RefreshListData(subAbility);

		var setType = UICommon.GetRuneSetAbilityText(DBItem.GetItem(data.RuneTid).RuneSetType);

		txtSetInfo.text = setType.Replace("\n", " ");

		RefreshAutoEnhanceUI();
	}

	private void RefreshCost(RuneEnchant_Table table)
	{
		cost = 0;

		if (isAutoEnhance == false)
			btnEnhance.interactable = table != null;
		else
		{
			btnEnhance.gameObject.SetActive(false);
			btnEnhance.interactable = false;
		}

		if (table != null)
			cost = table.EnchantItemCount;

		txtCost.text = cost.ToString("N0");
		txtCost.color = ConditionHelper.CheckCompareCost(DBConfig.Gold_ID, cost, false) == false ? Color.red : Color.white;
	}

	public void OnClickEnhance()
	{
		// 자동진행이고, 아직 시작안했다.
		if(isAutoEnhance && isAutoEnhancePlaying == false)
		{
			if(tableBaseEnchant.EnchantStep >= autoEnhanceTargetStep )
			{
				UIMessagePopup.ShowPopupOk(DBLocale.GetText("PR_Notice_Enhance_Already"));
				return;
			}

			UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("PR_Notice_EnhanceAuto"), () =>
			{
				isAutoEnhancePlaying = true;
				RefreshAutoEnhanceUI();

				OnClickEnhance();
			}, delegate { });
			return;
		}
		// 자동진행이 아니고, 연출 플레이중임 -> 중간에 중단됨
		else if (isAutoEnhance == false && isAutoEnhancePlaying)
		{
			isAutoEnhance = false;
			isAutoEnhancePlaying = false;

			RefreshAutoEnhanceUI();

			return;
		}
		else
		{
			if (ConditionHelper.CheckCompareCost(DBConfig.Gold_ID, cost) == false)
			{
				return;
			}
		}

		ZWebManager.Instance.WebGame.REQ_RuneEnchant(runeData.RuneId, (recvPacket, recvMsgpacket) =>
		{
			this.SafeStartCoroutine(CoShowFxResult(recvMsgpacket.EnchantRune.HasValue));
		});
	}

	private IEnumerator CoShowFxResult(bool isSuccess)
	{
		fxInteractable.SetInteractable(false);
		fxStart.Play();

		yield return new WaitForSeconds(fxStart.duration);

		float duration = 0f;
		if (isSuccess)
		{
			fxSuccess.Play();
			duration = fxSuccess.duration;
		}
		else
		{
			fxFail.Play();
			duration = fxFail.duration;
		}

		yield return new WaitForSeconds(duration);

		ResetFx();

		var newRune = Me.CurCharData.GetRune(runeData.RuneId);

		SetEnhanceEquip(newRune, onClose);
		
		// 자동강화가 아니라면 끝
		if(isAutoEnhance == false)
		{
			isAutoEnhance = false;
			isAutoEnhancePlaying = false;

			RefreshAutoEnhanceUI();

			yield break;
		}
		else if(isAutoEnhance && isAutoEnhancePlaying)
		{
			// 강화수치 체크
			if (tableBaseEnchant.EnchantStep>= autoEnhanceTargetStep)
			{
				isAutoEnhance = false;
				isAutoEnhancePlaying = false;

				RefreshAutoEnhanceUI();

				yield break;
			}

			// 다음강화~
			OnClickEnhance();
		}
	}

	// 자동강화와 관련된 UI 갱신해줌
	private void RefreshAutoEnhanceUI()
	{
		// 토글의 isOn 상태도 변경해줘야함

		btnEnhance.gameObject.SetActive(!isAutoEnhancePlaying);
		btnEnhance.interactable = !isAutoEnhancePlaying;

		toggleAutoEnhance.SelectToggleSingle(!isAutoEnhancePlaying, false);
		toggleAutoEnhance.interactable = !isAutoEnhancePlaying;

		toggleGroupAutoEnhance.GetAllToggle().ForEach(item => item.interactable = !isAutoEnhancePlaying);
		toggleGroupAutoEnhance.gameObject.SetActive(isAutoEnhance);

		btnStopAutoEnhance.gameObject.SetActive(isAutoEnhancePlaying);

		RefreshCost(tableNextEnchant);
	}

	private void ResetFx(bool isForce = false)
	{
		fxStart.Stop();
		fxSuccess.Stop();
		fxFail.Stop();
		fxInteractable.SetInteractable(true, isForce);
	}

	public void OnClickClose()
	{
		CancelInvoke();

		onClose?.Invoke();
		UIManager.Instance.Close<UIPopupEnhanceEquipPR>(true);
	}

	public void OnClickStopAutoEnhance()
	{
		isAutoEnhance = false;
	}

	public void OnClickAutoEnhance(bool state)
	{
		isAutoEnhance = state;

		toggleGroupAutoEnhance.gameObject.SetActive(state);

		if (state)
		{
			toggleGroupAutoEnhance.GetToggle(0).SelectToggle();
			OnClickEnhanceTargetStep(3);
		}
	}

	/// <summary>
	/// 자동장화 목표치
	/// </summary>
	/// <param name="_destStep"></param>
	public void OnClickEnhanceTargetStep(int _destStep)
	{
		autoEnhanceTargetStep = _destStep;
	}
}
