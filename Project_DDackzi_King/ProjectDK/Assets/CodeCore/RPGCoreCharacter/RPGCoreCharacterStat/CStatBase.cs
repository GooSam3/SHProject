using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStatOwner
{
	public void IStatUpdate(int hStatType, float fStatValue);
	public void IStatDie(CUnitBase pKiller);
}

abstract public class CStatBase
{
	protected int m_hStatType = 0;			
	private float m_fStatDefault = 0; // 시작시 주어지는 기본값
	private float m_fStatLimit = 0;   // Apply 한계값
	private float m_fStatValue = 0;   // 장비 버프등이 합산된 최종 적용 값
	private IStatOwner m_pStatOwner = null;

	private List<CStatBase> m_listChainStat = new List<CStatBase>(); // 이 스텟과 연관된 다른 스텟 
	private LinkedList<CStatModifierBase> m_listModifier = new LinkedList<CStatModifierBase>(); // 장비나 버프등에 의한 수정자 

	//------------------------------------------------------------------
	public void DoStatModifierRemove(CStatModifierBase pModifier)
	{
		m_listModifier.Remove(pModifier);
		PrivStatValueRefresh();
	}

	public void DoStatModifierAdd(CStatModifierBase pModifier)
	{
		m_listModifier.AddLast(pModifier);
		PrivStatValueRefresh();
	}

	public void DoStatValueAdd(float fValueAdd) // 레벨업등으로 기본 스텟 증가시
	{
		m_fStatDefault += fValueAdd;
		PrivStatValueRefresh();
	}

	public void DoStatValueReset(float fValueReset, float fLimitValue, IStatOwner pStatOwner) // 특정 값으로 초기화
	{
		m_fStatDefault = fValueReset;

		m_fStatDefault = fValueReset;
		m_fStatLimit = fLimitValue;
		m_pStatOwner = pStatOwner;

		if (fValueReset > fLimitValue)
		{
			m_fStatLimit = fValueReset;
		}
		PrivStatValueRefresh();
	}

	public int GetStatType() { return m_hStatType; }

	internal void ImportStatValueChain(int hStatID, float fValue)
	{
		OnStatValueChain(hStatID, fValue);
	}

	static public implicit operator float(CStatBase pStat)
	{
		return pStat.m_fStatValue;
	}
	//-----------------------------------------------------------------
	private void PrivStatValueRefresh()
	{
		float fValue = m_fStatDefault;

		foreach(CStatModifierBase pModifier in m_listModifier)
		{
			fValue += pModifier.Value;
		}

		PrivStatValueApply(fValue);
	}

	private void PrivStatValueApply(float fValue)
	{
		if (fValue > m_fStatLimit)
		{
			fValue = m_fStatLimit;
		}
		m_fStatValue = fValue;
		PrivStatValueNotifyChain(m_hStatType, m_fStatValue);
		m_pStatOwner?.IStatUpdate(m_hStatType, m_fStatValue);
	}

	private void PrivStatValueNotifyChain(int hStatID, float fValue)
	{
		for (int i = 0; i < m_listChainStat.Count; i++)
		{
			m_listChainStat[i].ImportStatValueChain(hStatID, fValue);
		}
	}

	//------------------------------------------------------------------
	protected virtual void OnStatValueRefresh(float fStatValue) { } // 나의 값이 변경되었을때 하부 통지 
	protected virtual void OnStatValueChain(int hStatType, float fApplyValue) { } // 나와 연관된 스텟이 변경되었을때 통지. 여기서 나의 스텟을 계산
}
