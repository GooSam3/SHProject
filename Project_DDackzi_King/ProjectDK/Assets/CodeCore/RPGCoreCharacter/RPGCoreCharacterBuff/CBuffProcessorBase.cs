using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBuffProcessorBase : CMonoBase
{
	private LinkedList<CBuffBase> m_listBuffInstance = new LinkedList<CBuffBase>();
	//------------------------------------------------------------------------------------------------
	private void Update()
	{
		OnUnityUpdate();
	}

	//----------------------------------------------------------------------------------------------------
	public void DoBuffProcessorBuffApply(CBuffBase pBuffInstance, CBuffProcessorBase pBuffOrigin, float fDuration, float fBuffPower)
	{
		CBuffBase pStackBuff = FindBuff(pBuffInstance.GetBuffID());
		if (pStackBuff != null)
		{

		}
		else
		{
			m_listBuffInstance.AddLast(pBuffInstance);
			pBuffInstance.DoBuffStart(this, pBuffOrigin,  fDuration, fBuffPower);
		}

		OnBuffProcessBuffStart(pBuffInstance);
	}

	//--------------------------------------------------------------------------------------------------
	public void ImportBuffEnd(CBuffBase pBuffEnd)
	{
		m_listBuffInstance.Remove(pBuffEnd);
		OnBuffProcessBuffEnd(pBuffEnd);
	}

	//--------------------------------------------------------------------------------------------------
	protected CBuffBase FindBuff(uint hBuffID)
	{
		CBuffBase pFindBuff = null;
		LinkedList<CBuffBase>.Enumerator it = m_listBuffInstance.GetEnumerator();
		while (it.MoveNext())
		{
			if (it.Current.GetBuffID() == hBuffID)
			{
				pFindBuff = it.Current;
				break;
			}
		}
		return pFindBuff;
	}

	//--------------------------------------------------------------------------------------------------
	private void PrivBuffProcessMergeBuff(CBuffBase pBuffStack, CBuffBase pBuffNew, float fDuration, float fPower)
	{

	}

	//----------------------------------------------------------------------------------------------------
	public CUnitBuffBase.SDamageResult ImportBuffProcessDamageCalculation(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		return OnBuffProcessDamageCalculation(pStatMe, pStatDest, rResult);
	}

	public CUnitBuffBase.SDamageResult ImportBuffProcessHealCalculation(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		return OnBuffProcessHealCalculation(pStatMe, pStatDest, rResult);
	}

	public CUnitBuffBase.SDamageResult ImportBuffProcessDamageApply(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		return OnBuffProcessDamageApply(pStatMe, pStatDest, rResult);
	}

	public CUnitBuffBase.SDamageResult ImportBuffProcessHeadApply(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		return OnBuffProcessHealApply(pStatMe, pStatDest, rResult);
	}

	//---------------------------------------------------------------
	protected virtual void OnUnityUpdate() { }
	protected virtual void OnBuffProcessBuffStart(CBuffBase pBuff) { }
	protected virtual void OnBuffProcessBuffEnd(CBuffBase pBuff) { }
	protected virtual void OnBuffProcessBuffMerge(CBuffBase pBuffOrigin, CBuffBase pBuffNew) { }
	protected virtual CUnitBuffBase.SDamageResult OnBuffProcessDamageCalculation(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult) { return rResult; }
	protected virtual CUnitBuffBase.SDamageResult OnBuffProcessHealCalculation(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult) { return rResult; }
	protected virtual CUnitBuffBase.SDamageResult OnBuffProcessDamageApply(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult) { return rResult; }
	protected virtual CUnitBuffBase.SDamageResult OnBuffProcessHealApply(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult) { return rResult; }

}
