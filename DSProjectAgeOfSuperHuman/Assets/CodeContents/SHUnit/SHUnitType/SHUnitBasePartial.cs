using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract partial class SHUnitBase : CUnitAIBase, ISHSkillProcessor
{
	private const string c_TagCoolTimeName = "TagCoolTime";
	private CCoolTime m_pTagCoolTimer = new CCoolTime();
	private uint m_iHPGaugeScale = 1; public uint GetUnitHPGaugeScale() { return m_iHPGaugeScale; } public void SetUnitHPGaugeScale(uint iHPGaugeScale) { m_iHPGaugeScale = iHPGaugeScale; }
	protected EUnitTagPosition m_eUnitTagPosition = EUnitTagPosition.None;				  public EUnitTagPosition GetUnitTagPositionType() { return m_eUnitTagPosition; }
	//-------------------------------------------------------------------------
	public void ISHSkillAttackTo(SHUnitBase pTarget, NSkill.EDamageType eDamageType, EUnitSocket eUnitSocket, string strHitEffectName, float fPower, List<SHStatModifier> pListBonusStat)
	{
		if (m_bAlive == false) return;
		
		SDamageResult pDamageResult = m_pSHStatComponent.DoStatDamageCalculation(pTarget.ExportUnitStat(), eDamageType, fPower, pListBonusStat);
		if (pDamageResult.eDamageType == NSkill.EDamageType.Heal)
		{
			EUnitState eUnitState = pTarget.GetUnitState();
			if (eUnitState == EUnitState.DeathStart || eUnitState == EUnitState.Death || eUnitState == EUnitState.Remove) return;
			pTarget.PrivUnitApplyHeal(pDamageResult, eUnitSocket, strHitEffectName);
		}
		else
		{
			if (pTarget.IsAlive == false) return;
			pTarget.PrivUnitApplyDamage(pDamageResult, eUnitSocket, strHitEffectName);
		}
	}

	public void ISHSetTagCoolTime(float fCoolTime)
	{
		m_pTagCoolTimer.SetCoolTime(c_TagCoolTimeName, fCoolTime);
	}

	public float ISHGetTagCoolTime()
	{
		return m_pTagCoolTimer.GetCoolTime(c_TagCoolTimeName);
	}

	public void ISHAnimSkinChange(string strAniGroupName, string strSkinName)
	{
		m_pSHAnimationSpine.DoAnimationSkinChange(strAniGroupName, strSkinName);
	}

	public void ISHSkillRageGain(float fRagePoint)
	{
		OnSHUnitGainRage(fRagePoint);
	}

	//-------------------------------------------------------------------------
	public Vector2 GetUnitHP()
	{
		return m_pSHStatComponent.GetStatHP();
	}

	public float GetUnitPower()
	{
		float fPower = 0f;

		return fPower;
	}

	public int GetUnitLevel()
	{
		return m_pSHStatComponent.GetStatLevel();
	}

	public bool GetUnitAIEnable()
	{
		return m_pSHCombatAI.GetCombatAIEnable();
	}

	public Transform GetUnitSocketTransform(EUnitSocket eSocketID)
	{
		SHUnitSocketAssist pSocketAssist = m_pSocketAssist as SHUnitSocketAssist;
		return pSocketAssist.GetUnitSocketTrasform(eSocketID);
	}

	public List<SHUnitSkillFSMBase.SSkillExportInfo> ExportUnitSkillInfo()
	{
		return m_pSHSkillFSM.ExportSkillInfo();
	}

	public void SetUnitInfo(uint hUnitID, string strUnitName, int iLevel)
	{
		SetUnitID(hUnitID, strUnitName);
		m_pSHStatComponent.DoStatID(hUnitID, iLevel);
	}

	//---------------------------------------------------------------------------
	public NBuff.EBuffType GetUnitCrowdControll()
	{
		return m_pSHBuffComponent.HasBuffCrowdControll();
	}

	//-----------------------------------------------------------------------
	private void PrivUnitApplyDamage(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHitEffectName)
	{
		m_pSHStatComponent.DoStatApplyDamage(pDamageResult);
		OnSHUnitApplyDamage(pDamageResult, eUnitSocket, strHitEffectName);
	}

	private void PrivUnitApplyHeal(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHitEffectName)
	{		
		m_pSHStatComponent.DoStatApplyHeal(pDamageResult);
		OnSHUnitApplyHeal(pDamageResult, eUnitSocket, strHitEffectName);
	}

	//------------------------------------------------------------------------
	protected virtual void OnSHUnitApplyDamage(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHittEffectName) { }
	protected virtual void OnSHUnitApplyHeal(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHitEffectName) { }
	protected virtual void OnSHUnitGainRage(float fRagePoint) { }
}
