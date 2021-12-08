using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHCombatAIBase : CCombatAIBase
{
	[SerializeField]
	private float DelayStart = 1f;
	[SerializeField]
	private float DelayNormalAttack = 1f;
	[SerializeField]
	private float DelaySkillSlot = 1f;

	private float		m_fDelayStartCurrent = 0;
	private float		m_fDelaySkillSlotCurrent = 0;
	private float		m_fDelayNormalAttackCurrent = 0;
	private bool		m_bDelayStart = false;
	protected SHUnitBase m_pSHUnit = null;

	protected List<SHUnitSkillFSMBase.SSkillExportInfo> m_listSkillExportInfo = new List<SHUnitSkillFSMBase.SSkillExportInfo>();
	//--------------------------------------------------------
	protected override void OnCombatAIProcessor(ICombatAIProcessor pCombatAIProcessor)
	{
		base.OnCombatAIProcessor(pCombatAIProcessor);
		m_pSHUnit = pCombatAIProcessor.IGetCombatUnit() as SHUnitBase;
	}

	protected override void OnCombatAIUpdate()
	{
		base.OnCombatAIUpdate();

		if (m_pSHUnit.IsAlive == false) return;

		float fDelta = Time.deltaTime; 

		if (m_bDelayStart == false)
		{
			UpdateCombatAIDelayStart(fDelta);
		}
		else
		{
			if (CheckCombatAICrowdControll())
			{
				return;
			}

			if (CheckCombatAIGlobalCoolTime())
			{
				return;
			}

			UpdateCombatAISkillSlot(fDelta);
		}
	}

	protected override void OnCombatAIReset()
	{
		base.OnCombatAIReset();
		m_fDelayStartCurrent = 0f;
	}

	//----------------------------------------------------------
	public void DoCombatAIStart()
	{
		SetCombatAIEnable(true);
		m_listSkillExportInfo = m_pSHUnit.ExportUnitSkillInfo();
	}

	public void DoCombatAIEnd()
	{
		SetCombatAIEnable(false);
	}

	//--------------------------------------------------------
	protected void ProtCombatAIReset()
	{
		m_fDelayStartCurrent = 0;
		m_fDelaySkillSlotCurrent = 0;
		m_fDelayNormalAttackCurrent = DelayNormalAttack;
		m_bDelayStart = false;
	}

	//---------------------------------------------------------
	private void UpdateCombatAIDelayStart(float fDelta)
	{
		m_fDelayStartCurrent += fDelta;
		if (m_fDelayStartCurrent > DelayStart)
		{
			m_bDelayStart = true;
		}
	}

	private void UpdateCombatAISkillSlot(float fDelta)
	{
		m_fDelaySkillSlotCurrent += fDelta;		
		if (m_fDelaySkillSlotCurrent >= DelaySkillSlot)
		{
			if (OnSHCombatAISkillSlotTry() == false)
			{
				UpdateCombatAISkillNormal(fDelta);
			}
			else
			{
				m_fDelaySkillSlotCurrent = 0f;
			}
		}
		else
		{
			UpdateCombatAISkillNormal(fDelta);
		}
	}

	private void UpdateCombatAISkillNormal(float fDelta)
	{
		m_fDelayNormalAttackCurrent += fDelta;
		if (m_fDelayNormalAttackCurrent >= DelayNormalAttack)
		{
			m_fDelayNormalAttackCurrent = 0;
			OnSHCombatAISkillNormal();
		}
	}

	//----------------------------------------------------------
	private bool CheckCombatAIGlobalCoolTime()
	{
		bool bGlobalCooltime = false;
		float fGC = m_pSHUnit.IGetSkillCoolTimeGlobal(SHSkillUsage.g_GlobalCoolTime);
		if (fGC > 0)
		{
			bGlobalCooltime = true;
		}
		return bGlobalCooltime;
	}

	private bool CheckCombatAICrowdControll()
	{
		bool bFind = false;
		NBuff.EBuffType eBuffType = m_pSHUnit.GetUnitCrowdControll();
		if (eBuffType == NBuff.EBuffType.CrowdControl)
		{
			bFind = true;
		}
		return bFind;
	}

	//----------------------------------------------------------------
	protected virtual bool OnSHCombatAISkillSlotTry() { return false; }
	protected virtual void OnSHCombatAISkillNormal() { }
}
