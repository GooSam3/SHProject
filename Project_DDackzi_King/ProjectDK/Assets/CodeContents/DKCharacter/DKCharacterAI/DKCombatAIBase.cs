using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDKCombatAIProcessor : ICombatAIProcessor
{
	public uint		IAIGetSkillReadyToUse(EActiveSkillType eActiveSkillType);
	public void		IAIUseSkill(uint hSkillID, CSkillUsage pUsage);
	public float		IAIGetUnitStat(EDKStatType eStatType);
}

public abstract class DKCombatAIBase : CCombatAIBase
{
	[SerializeField]
	private uint TargetResetCount = 3;    // 일반공격 타겟이 리셋된다.

	protected EClassType m_eClassType = EClassType.None;
    protected IDKCombatAIProcessor m_pAIProcessor = null;
	protected DKUnitBase	m_pTargetUnit = null;
	protected uint		m_iTargetCount = 0;
	//----------------------------------------------------------
	protected sealed override void OnCombatAIProcessor(ICombatAIProcessor pCombatAIProcessor)
	{
		m_pAIProcessor = pCombatAIProcessor as IDKCombatAIProcessor;
	}

	protected override void OnCombatAIUpdate()
	{
		base.OnCombatAIUpdate();

		if (m_pAIProcessor.IHasSkillPlay() == false)
		{
			PrivCombatAISkillUse();
		}
	}
	//-----------------------------------------------------------
	protected void ProtCombatAIExtractFormationMember(List<DKFormationBase.SFomationMember> pFormationMember, EDKStatType eStatType, bool bSortHigh)
	{
		DKManagerStageSpawner.Instance.ExtractFormationUnit(false, pFormationMember);

		if (pFormationMember.Count == 0) return;

		pFormationMember.Sort((rFormationA, rFormationB) =>
		{
			int Result = 0;
			if (bSortHigh)
			{
				Result = rFormationA.pMember.IAIGetUnitStat(EDKStatType.AttackPower).CompareTo(rFormationB.pMember.IAIGetUnitStat(EDKStatType.AttackPower));
			}
			else
			{
				Result = rFormationB.pMember.IAIGetUnitStat(EDKStatType.AttackPower).CompareTo(rFormationA.pMember.IAIGetUnitStat(EDKStatType.AttackPower));
			}

			return Result;
		});
	}

	//-----------------------------------------------------------
	private void PrivCombatAISkillUse()
	{
		// 궁극기부터 사용
		if (PrivCombatAISkillUseSkillType(EActiveSkillType.SkillUltimate))
		{
			return;
		}

		if (PrivCombatAISkillUseSkillType(EActiveSkillType.SkillCondition))
		{
			return;
		}

		if (PrivCombatAISkillUseSkillType(EActiveSkillType.SkillActive))
		{
			return;
		}

		if (PrivCombatAISkillUseSkillType(EActiveSkillType.SkillNormal))
		{
			return;
		}
	}

	private bool PrivCombatAISkillUseSkillType(EActiveSkillType eSkillType)
	{
		bool bUseSkill = false;
		uint hSkillID = m_pAIProcessor.IAIGetSkillReadyToUse(eSkillType);
		if (hSkillID != 0)
		{
			DKUnitBase pTargetUnit = PrivCombatSkillTarget(eSkillType);

			if (pTargetUnit == null)
			{
				return false;
			}

			CSkillUsage pUsage = new DKSkillUsage();
			pUsage.UsageSkillID = hSkillID;

			pUsage.UsageTarget = pTargetUnit;
			pUsage.UsageOrigin = m_pAIProcessor.IGetCombatUnit().GetUnitPosition();

			if (pTargetUnit != null)
			{
				pUsage.UsagePosition = pTargetUnit.GetUnitPosition();
				pUsage.UsageDirection = pUsage.UsagePosition - pUsage.UsageOrigin;
			}

			m_pAIProcessor.IAIUseSkill(hSkillID, pUsage);
			bUseSkill = true;
		}

		return bUseSkill;
	}

	private DKUnitBase PrivCombatSkillTarget(EActiveSkillType eSkillType)
	{
		DKUnitBase pTarget = null;
		
		if (m_pTargetUnit != null)
		{
			if (m_pTargetUnit.IsAlive)
			{
				pTarget = m_pTargetUnit;
			}
		}

		if (pTarget == null)
		{
			m_pTargetUnit = OnCombatAIFindTarget(eSkillType);
			pTarget = m_pTargetUnit;
		}
		else if (eSkillType == EActiveSkillType.SkillNormal) // 노멀어택 횟수만큼 공격하면 타겟이 리셋 
		{
			m_iTargetCount++;
			if (m_iTargetCount >= TargetResetCount)
			{
				DKManagerStageSpawner.Instance.DoMgrStageSpawnerDecreaseAggro(m_pTargetUnit); // 이전 타겟은 어그로 내림
				m_pTargetUnit = null;
				m_iTargetCount = 0;
			}
		}

		return pTarget;
	}

	//-----------------------------------------------------------
	protected virtual void OnCombatAISkillUse(EActiveSkillType eSkillType, DKUnitBase pTarget) {}
	protected virtual DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType) { return null; }
}
